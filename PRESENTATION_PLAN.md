# Plan for DLS Exam Presentations

## Oversigt
Dette dokument indeholder en plan for hver af de 12 eksamensspørgsmål, inkluderet hvilke demonstrationer der kan laves med denne codebase og hvilke der kræver ekstra arbejde eller eksterne eksempler.

---

## Spørgsmål 1: Asynkron design

### Teoretisk indhold
- **Problemstilling**: Blokering af tråde under I/O-operationer (database, HTTP calls, messaging)
- **Løsning**: Async/await pattern i C#, event-driven messaging via RabbitMQ
- **Kompromiser**: Øget kompleksitet (error handling, cancellation tokens), debugging udfordringer

### Demonstrationer fra codebase
✅ **Kan demonstreres direkte:**

1. **Async messaging med RabbitMQ**
   - **Fil**: `PublisherService/Services/PublishingService.cs` (linje 14-33)
   - **Demonstration**: Vis hvordan `PublishAsync` sender beskeder asynkront uden at blokere
   - **Fil**: `ArticleService/Services/PublishingConsumer.cs` (linje 29-45)
   - **Demonstration**: Vis hvordan consumer håndterer beskeder asynkront i baggrunden
   - **Performance test**: Send 100 artikler og vis at API returnerer med det samme, mens processing sker asynkront

2. **Async database operations**
   - **Fil**: `ArticleService/database.cs` (find async metoder)
   - **Demonstration**: Sammenlign synkron vs asynkron database kald med performance metrics
   - **Test**: Kald `GetArticle` med async vs sync og vis response time forskel

3. **Async cache operations**
   - **Fil**: `ArticleService/Services/RedisCacheService.cs`
   - **Demonstration**: Vis async cache reads/writes der ikke blokerer request thread

### Performance demonstration
- **Setup**: Brug load testing tool (Apache Bench, k6, eller Postman)
- **Test 1**: Send 100 concurrent requests til synkron endpoint (hvis du laver en)
- **Test 2**: Send 100 concurrent requests til async endpoint
- **Vis**: Response times, throughput, thread pool usage

### Kompromiser diskussion
- Vis kode eksempler på:
  - Error handling kompleksitet (`try-catch` i async contexts)
  - Cancellation token håndtering
  - Deadlock potential (`.GetAwaiter().GetResult()` anti-pattern i `CommentManager.cs` linje 40)

### Anbefaling
**Branch**: `demo/async-design`
- Tilføj en synkron version af en endpoint for sammenligning
- Tilføj performance logging/metrics
- Dokumenter thread pool usage

---

## Spørgsmål 2: Fault isolation

### Teoretisk indhold
- **Principper**: Isolation boundaries, failure containment, graceful degradation
- **Circuit breaker pattern**: Automatisk isolation ved gentagne fejl
- **Resilience patterns**: Retry, fallback, circuit breaker, timeout

### Demonstrationer fra codebase
✅ **Kan demonstreres direkte:**

1. **Circuit breaker implementation**
   - **Fil**: `CommentService/CommentManager.cs` (linje 15-23)
   - **Fil**: `CommentService/Repositories/CommentDbRepository.cs` (linje 18-26)
   - **Fil**: `NewsletterService/Program.cs` (linje 25-40)
   - **Demonstration**:
     - Stop ProfanityService container
     - Vis hvordan circuit breaker åbner efter 4 fejl
     - Vis console logs: "Circuit breaker tripped"
     - Genstart service og vis "Circuit breaker reset"
     - Vis hvordan CommentService fortsætter at fungere (isolation)

2. **Retry pattern**
   - **Fil**: `CommentService/CommentManager.cs` (linje 11-13)
   - **Fil**: `NewsletterService/Program.cs` (linje 16-23)
   - **Demonstration**: Simuler transient fejl og vis exponential backoff retry

3. **Fault isolation i microservices**
   - **Arkitektur**: CommentService → ProfanityService
   - **Demonstration**: 
     - Stop ProfanityService → CommentService isolerer fejlen
     - Stop CommentService → ArticleService fortsætter
     - Vis hvordan hver service har sin egen database (isolation)

### Sammenligning af resilience patterns
**Demonstration setup:**
1. **Retry only**: Vis exponential backoff i `CommentManager.cs`
2. **Circuit breaker**: Vis i `NewsletterService/Program.cs`
3. **Fallback**: Ikke implementeret - **Tilføj eksempel**
   - Tilføj fallback til default response når ProfanityService er nede

### Anbefaling
**Branch**: `demo/fault-isolation`
- Tilføj fallback pattern eksempel
- Tilføj timeout pattern
- Tilføj health check endpoints
- Dokumenter availability metrics før/efter circuit breaker

---

## Spørgsmål 3: Skaleringskuben

### Teoretisk indhold
- **X-akse**: Cloning/replikation (load balancing)
- **Y-akse**: Functional decomposition (microservices)
- **Z-akse**: Data partitioning (sharding)
- **Headroom**: Kapacitetsplanlægning med buffer

### Demonstrationer fra codebase
✅ **Kan demonstreres direkte:**

1. **Z-akse skalering (Database sharding)**
   - **Fil**: `ArticleService/coordinator.cs` (linje 21-35)
   - **Fil**: `docker-compose.yml` (linje 66-169) - 8 regionale databaser
   - **Demonstration**:
     - Vis hvordan artikler distribueres til regionale databaser (Africa, Asia, Europe, etc.)
     - Vis `GetConnectionByRegion` metode
     - Vis hvordan queries routes til korrekt database
     - **Performance**: Vis hvordan queries er hurtigere når data er partitioneret

2. **X-akse skalering (Replikation)**
   - **Fil**: `docker-compose.yml` (linje 50-51) - `replicas: 3` for ArticleService
   - **Demonstration**: Vis 3 instanser af ArticleService kører samtidigt
   - **Load balancing**: Test med multiple requests og vis distribution

3. **Y-akse skalering (Microservices)**
   - **Arkitektur**: ArticleService, CommentService, DraftService, NewsletterService, etc.
   - **Demonstration**: Vis funktionel opdeling

### Headroom beregninger
**Eksempel konstruktion:**
- **Setup**: 
  - Current load: 1000 requests/min
  - Peak load: 5000 requests/min
  - Headroom: 20% buffer = 6000 requests/min capacity needed
- **Beregn**: Hvor mange instanser/databaser er nødvendige
- **Vis**: Kapacitetsplanlægning baseret på:
  - Database connection limits
  - Container resources
  - Network bandwidth

### Kombination af to akser
**Eksempel: X + Z skalering**
- **Problem**: ArticleService skal håndtere høj load OG store mængder data
- **Løsning**: 
  - X-akse: 3 replikaer af ArticleService (load distribution)
  - Z-akse: 8 shardede databaser (data distribution)
- **Demonstration**: Vis hvordan systemet skalerer både horisontalt og vertikalt

### Kompromiser ved Z-akse skalering
**Diskussion baseret på codebase:**
- **Cross-shard queries**: Svært at query alle regioner (se `GetAllConnections` i coordinator.cs)
- **Data consistency**: Hvordan håndteres consistency mellem shards?
- **Migration complexity**: Hvordan flyttes data mellem shards?
- **Transaction management**: Distributed transactions er komplekse

### Anbefaling
**Branch**: `demo/scaling-cube`
- Tilføj load testing script
- Dokumenter headroom beregninger
- Vis cross-shard query eksempel
- Tilføj monitoring af shard distribution

---

## Spørgsmål 4: Green Architecture Framework

### Teoretisk indhold
- **GAF principper**: Energy efficiency, resource optimization, sustainable design
- **Taktikker**: Caching, lazy loading, connection pooling, efficient algorithms
- **Skalerbarhed**: GAF kan forbedre skalerbarhed gennem resource efficiency

### Demonstrationer fra codebase
✅ **Delvist kan demonstreres:**

1. **Caching taktikker**
   - **Fil**: `ArticleService/Services/RedisCacheService.cs`
   - **Fil**: `CommentService/Services/CommentCacheService.cs`
   - **Demonstration**: 
     - Vis cache hit/miss rates
     - Vis reduktion i database calls
     - Vis energy savings gennem færre database operations

2. **Connection pooling**
   - **Fil**: `ArticleService/coordinator.cs` (linje 10, 49-60)
   - **Demonstration**: Vis connection caching/reuse

3. **Efficient data structures**
   - **Fil**: `ArticleService/Services/CacheUpdaterService.cs`
   - **Demonstration**: Vis offline cache updates (background worker)

### Før/efter eksempler
**Tilføj demonstration:**
- **Før**: Query database for hver request
- **Efter**: Cache med Redis, query kun ved cache miss
- **Måling**: CPU usage, memory usage, response time

### GAF og skalerbarhed
- **Diskussion**: Hvordan gør caching systemet mere skalerbart?
  - Reducerer database load → flere requests kan håndteres
  - Reducerer network traffic → bedre throughput
  - Reducerer compute requirements → lavere costs

### Anbefaling
**Branch**: `demo/green-architecture`
- Tilføj metrics collection for energy/resource usage
- Sammenlign med/uden caching
- Dokumenter GAF taktikker i codebase
- **Note**: Dette spørgsmål kræver måske mere teoretisk fokus, da GAF ikke er direkte implementeret

---

## Spørgsmål 5: Automation og DevOps

### Teoretisk indhold
- **"Automation over people"**: Midt i Venn-diagram (skalerbarhed, konsistens, hastighed)
- **CI/CD pipeline**: Automatiseret build, test, deploy
- **DevOps principper**: Infrastructure as Code, containerization, monitoring

### Demonstrationer fra codebase
✅ **Delvist kan demonstreres:**

1. **Containerization (Docker)**
   - **Filer**: Alle services har `Dockerfile`
   - **Fil**: `docker-compose.yml` - Infrastructure as Code
   - **Demonstration**:
     - Vis Dockerfiles
     - Vis docker-compose orchestration
     - Vis hvordan hele systemet kan startes med `docker-compose up`
     - Vis konsistent miljø (dev = prod)

2. **Infrastructure as Code**
   - **Fil**: `docker-compose.yml`
   - **Demonstration**: Vis hvordan hele infrastruktur er defineret i kode

### CI/CD Pipeline
✅ **Fuldt implementeret!**

**Eksisterende pipeline:**
- **Filer**: `.github/workflows/*.yml` (7 workflows - én per service)
- **Pipeline flow**:
  1. Code commit → GitHub (trigger på push til main)
  2. Automated build (Docker image)
  3. Push to registry (ghcr.io) med version tag og latest tag
  4. Path-based triggers (kun bygger når relevant service ændres)

**Demonstration setup:**
- **Filer**: `.github/workflows/article.yml`, `publisher.yml`, `newsletter.yml`, etc.
- **Vis**: 
  - Build step (Docker build)
  - Push step (to GitHub Container Registry)
  - Version tagging (`IMAGE_VERSION: ${{ github.run_number }}`)
  - Path-based triggers (eliminerer unødvendige builds)
  - Elimination af manuelle steps (fuldt automatiseret)

**Eksempel workflow:**
```yaml
- Triggers når ArticleService/** ændres
- Bygger Docker image
- Pusher til ghcr.io/lazh01/articleservice:123 (version)
- Pusher til ghcr.io/lazh01/articleservice:latest
```

### DevOps og brancher
- **Diskussion**: Hvordan gælder DevOps principper forskelligt for:
  - Tech startups (høj hastighed)
  - Enterprise (konsistens, compliance)
  - E-commerce (availability, skalering)

### Anbefaling
**Branch**: `demo/devops-automation`
- Vis eksisterende GitHub Actions workflows
- Demonstrer pipeline execution (via GitHub UI)
- Vis version tagging og rollback muligheder
- Dokumenter automation benefits
- Sammenlign manuel vs automatiseret deployment
- **Note**: CI/CD er allerede implementeret - fokus på demonstration!

---

## Spørgsmål 6: Recovery

### Teoretisk indhold
- **Feature flags**: Gradual rollout, instant disable
- **Rollback mekanismer**: Version control, database migrations, container rollback
- **"Design to be disabled"**: Services kan deaktiveres uden at påvirke systemet
- **"Design for rollback"**: Systemet kan rulles tilbage til tidligere version

### Demonstrationer fra codebase
✅ **Kan demonstreres:**

1. **Feature flags**
   - **Fil**: `SubscriberService/Services/SubscriptionService.cs` (linje 26-33)
   - **Fil**: `SubscriberService/Program.cs` (linje 16-37) - FeatureHub integration
   - **Demonstration**:
     - Vis FeatureHub integration
     - Vis `EnsureFeatureEnabled()` check
     - Deaktiver feature flag i FeatureHub UI
     - Vis hvordan service returnerer error når disabled
     - Genaktiver og vis service fungerer igen
     - **Use case**: Gradual rollout, A/B testing, emergency disable

2. **Design to be disabled**
   - **Arkitektur**: NewsletterService kan deaktiveres uden at påvirke andre services
   - **Demonstration**: Stop NewsletterService og vis at resten af systemet fungerer

### Rollback mekanismer
✅ **Delvist implementeret:**

1. **Container rollback (via CI/CD versioning)**: 
   - **Fil**: `.github/workflows/*.yml` (linje 18) - `IMAGE_VERSION: ${{ github.run_number }}`
   - **Demonstration**: 
     - Hver build får unikt version nummer
     - Alle versioner gemmes i GitHub Container Registry
     - Rollback: Skift image tag i docker-compose.yml til tidligere version
     - **Eksempel**: `ghcr.io/lazh01/articleservice:120` → `ghcr.io/lazh01/articleservice:119`
   
2. **Database migration rollback**
   - **Fil**: `DraftService/Migrations/` - EF Core migrations
   - **Demonstration**: Vis migration og rollback med `dotnet ef database update <PreviousMigration>`

3. **CI/CD rollback support**
   - Version tagging gør rollback muligt
   - Manual rollback: Opdater docker-compose.yml med tidligere version
   - **Forbedring mulighed**: Automatisk rollback ved health check failure

### Sammenligning: Design to be disabled vs Design for rollback
- **Design to be disabled**: 
  - Feature flags (SubscriberService eksempel)
  - Service kan deaktiveres runtime
  - Ingen deployment nødvendig
  
- **Design for rollback**:
  - Version control (Git tags)
  - Container versioning
  - Database migration reversibility
  - Deployment pipeline support

### Anbefaling
**Branch**: `demo/recovery`
- Demonstrer rollback via version tags fra CI/CD
- Dokumenter feature flag usage
- Vis database migration rollback
- Sammenlign de to principper med konkrete eksempler
- **Note**: Rollback infrastructure er på plads via versioning - fokus på demonstration!

---

## Spørgsmål 7: Specifikationer

### Teoretisk indhold
- **Formelle specifikationer**: Præcise, verificerbare, matematiske/logiske
- **Uformelle specifikationer**: Naturligt sprog, use cases, user stories
- **Outsourcing kontekst**: Klarhed, misforståelser, kontrakter

### Demonstrationer fra codebase
❌ **Ikke direkte relevant - kræver eksternt eksempel**

### Anbefalet tilgang
**Opret eksempel specifikation:**

**Formel specifikation eksempel:**
- **Funktion**: Profanity check endpoint
- **Input**: `{ text: string }`
- **Output**: `{ containsProfanity: boolean }`
- **Preconditions**: text != null, text.length > 0
- **Postconditions**: containsProfanity == true IFF text contains profanity words
- **Formel notation**: Brug matematisk/logisk notation

**Uformel specifikation:**
- "The service should check if text contains bad words and return true if it does"

**Sammenligning:**
- **Fordele formel**: Præcis, verificerbar, ingen tvetydighed
- **Ulemper formel**: Kompleks, kræver ekspertise, tidskrævende
- **Fordele uformel**: Let at forstå, hurtig at skrive
- **Ulemper uformel**: Tvetydig, kan misforstås, svær at verificere

### Anbefaling
**Branch**: `demo/specifications`
- Opret formel specifikation for ProfanityService endpoint
- Opret uformel version
- Sammenlign og diskuter
- **Note**: Dette spørgsmål er primært teoretisk, men kan bruge ProfanityService som case

---

## Spørgsmål 8: Design to be monitored

### Teoretisk indhold
- **Metrics**: Aggregerede mål (request rate, error rate, latency)
- **Logging**: Event-baserede beskeder (structured logging)
- **Tracing**: Request flow gennem systemet (distributed tracing)
- **Y-akse skalering problem**: Flere services = sværere at monitorere

### Demonstrationer fra codebase
✅ **Kan demonstreres direkte:**

1. **Monitoring setup**
   - **Filer**: Alle services har `Services/MonitorService.cs`
   - **Teknologier**: 
     - OpenTelemetry (tracing) → Zipkin
     - Serilog (logging) → Seq, Loki
     - Grafana (visualization)
   - **Demonstration**:
     - Vis Zipkin traces gennem systemet
     - Vis Seq logs fra forskellige services
     - Vis Grafana dashboards

2. **Structured logging**
   - **Fil**: `ArticleService/Services/RedisCacheService.cs` (linje 22-31)
   - **Demonstration**: Vis structured logs med properties (ArticleId, cache hit/miss)
   - **Fil**: `PublisherService/Services/PublishingService.cs` (linje 28, 30)
   - **Demonstration**: Vis logging med context (Title, Author)

3. **Distributed tracing**
   - **Fil**: `PublisherService/Services/PublishingService.cs` (linje 17-25)
   - **Demonstration**: 
     - Vis trace context injection i messages
     - Vis trace flow: PublisherService → ArticleService → NewsletterService
     - Vis spans i Zipkin UI

4. **Metrics fra logs/traces**
   - **Eksempel**: 
     - Parse logs for cache hit rate
     - Parse traces for service latency
     - Aggregate error rates fra logs

### Y-akse skalering problem
**Demonstration:**
- **Før**: Monolitisk system - alle logs i ét sted
- **Efter**: Microservices - logs spredt over mange services
- **Problem**: 
  - Hvordan finder man en fejl der spænder over flere services?
  - Hvordan aggregerer man metrics?
- **Løsning**: 
  - Distributed tracing (Zipkin)
  - Centralized logging (Seq, Loki)
  - Correlation IDs

### Anbefaling
**Branch**: `demo/monitoring`
- Vis Zipkin trace gennem hele systemet
- Vis metrics extraction fra logs
- Dokumenter monitoring challenges ved microservices
- Vis hvordan tracing løser Y-akse problem

---

## Spørgsmål 9: Organisatorisk skalering

### Teoretisk indhold
- **Udfordringer ved team vækst**: Communication overhead, coordination, knowledge silos
- **Risiko-analyse**: Identificer risici ved framework skift
- **Go/no-go beslutning**: Baseret på risiko-analyse

### Demonstrationer fra codebase
❌ **Ikke direkte relevant - primært teoretisk**

### Anbefalet tilgang
**Konstruer risiko-analyse:**

**Scenario**: Skift fra Scrum til SAFe (Scaled Agile Framework)

**Risiko-analyse:**
1. **Organisatoriske risici**:
   - Team modstand mod nyt framework
   - Træningsbehov
   - Ændring i roller og ansvar
   
2. **Tekniske risici**:
   - Ændring i development process
   - Integration med eksisterende tools
   - Impact på CI/CD pipeline
   
3. **Business risici**:
   - Productivity dip under transition
   - Project delays
   - Cost af træning og konsulenter

**Go/no-go beslutning:**
- **Kriterier**: 
  - Team størrelse (SAFE kræver 50+ personer)
  - Current pain points
  - Readiness assessment
  - Cost-benefit analysis

### Anbefaling
**Branch**: `demo/organizational-scaling`
- Opret risiko-analyse dokument
- Vis go/no-go decision framework
- Diskuter codebase impact (hvis relevant)
- **Note**: Dette er primært teoretisk, men kan relateres til codebase complexity

---

## Spørgsmål 10: Availability

### Teoretisk indhold
- **Availability**: Systemet er tilgængeligt når det skal bruges
- **Relatering til skaleringskuben**: X-akse (replikation) forbedrer availability
- **Mekanismer**: Replication, load balancing, health checks, failover
- **Negative arkitektoniske valg**: Single point of failure, tight coupling

### Demonstrationer fra codebase
✅ **Kan demonstreres:**

1. **Replication for availability**
   - **Fil**: `docker-compose.yml` (linje 50-51) - 3 replikaer af ArticleService
   - **Demonstration**: 
     - Stop én instans
     - Vis at systemet fortsætter (andre replikaer håndterer load)
     - Vis availability forbedring

2. **Health checks**
   - **Fil**: `docker-compose.yml` (linje 57-61, 68-73) - health checks for Redis og databases
   - **Demonstration**: Vis hvordan services venter på dependencies

3. **Database replication (implicit)**
   - **Fil**: `docker-compose.yml` - Multiple databases (sharding)
   - **Demonstration**: Hvis én database fejler, påvirker det kun én region

### Mekanismer til forbedring
**Demonstration setup:**
1. **Load balancing**: Vis distribution af requests til replikaer
2. **Circuit breakers**: (se Spørgsmål 2) - forhindrer cascade failures
3. **Retry mechanisms**: (se Spørgsmål 2) - håndterer transient failures

### Negative arkitektoniske valg
**Diskussion baseret på codebase:**
1. **Single RabbitMQ instance**: 
   - **Fil**: `docker-compose.yml` (linje 2-13)
   - **Problem**: Hvis RabbitMQ fejler, stopper hele messaging
   - **Løsning**: RabbitMQ cluster (ikke implementeret)

2. **Tight coupling**:
   - CommentService afhænger af ProfanityService
   - **Problem**: Hvis ProfanityService fejler, kan CommentService ikke fungere
   - **Løsning**: Circuit breaker (allerede implementeret!)

3. **No database backup strategy**: 
   - Ingen backup/replication strategy synlig
   - **Problem**: Data loss ved database failure

### Anbefaling
**Branch**: `demo/availability`
- Test failover scenarier
- Dokumenter availability metrics
- Vis negative patterns og løsninger
- Tilføj availability monitoring

---

## Spørgsmål 11: Time to market

### Teoretisk indhold
- **Time to market**: Tid fra idé til produktion
- **Relatering til skaleringsprincipper**: Automation, microservices, CI/CD
- **Mekanismer**: Feature flags, parallel development, independent deployment
- **Negative valg**: Monolitiske systemer, manual processes, tight coupling

### Demonstrationer fra codebase
✅ **Kan demonstreres:**

1. **Microservices for parallel development**
   - **Arkitektur**: Separate services kan udvikles uafhængigt
   - **Demonstration**: 
     - Vis hvordan ArticleService og CommentService kan deployes uafhængigt
     - Vis hvordan teams kan arbejde parallelt
     - Vis reduced coordination overhead

2. **Feature flags for gradual rollout**
   - **Fil**: `SubscriberService/Services/SubscriptionService.cs`
   - **Demonstration**: 
     - Deploy feature til produktion
     - Aktiver for subset af users
     - Gradual rollout uden deployment

3. **Independent deployment**
   - **Fil**: `docker-compose.yml` - Separate containers
   - **Demonstration**: Deploy én service uden at påvirke andre

### Mekanismer til forbedring
**Demonstration:**
1. **CI/CD automation**: 
   - **Fil**: `.github/workflows/*.yml` - Automatiseret build og push
   - **Benefit**: Reducerer deployment time fra timer til minutter
   - **Demonstration**: Vis workflow execution time
2. **Containerization**: Konsistent environments → færre bugs → hurtigere releases
3. **Async messaging**: Løs coupling → hurtigere development cycles
4. **Independent service deployment**:
   - **Fil**: `.github/workflows/*.yml` - Path-based triggers
   - **Benefit**: Deploy én service uden at påvirke andre → hurtigere releases

### Negative arkitektoniske valg
**Diskussion:**
1. **Tight coupling**: 
   - CommentService → ProfanityService
   - **Problem**: Kan ikke deploye CommentService uafhængigt
   - **Impact**: Længere time to market

2. **Shared database**:
   - Hvis flere services deler database
   - **Problem**: Database migrations blokerer alle services
   - **Løsning**: Separate databases (allerede implementeret!)

3. **Manual deployment**:
   - Hvis ingen CI/CD
   - **Problem**: Langsomme, fejlbehæftede releases
   - **Løsning**: Automation (se Spørgsmål 5)

### Anbefaling
**Branch**: `demo/time-to-market`
- Dokumenter deployment frequency
- Vis parallel development benefits
- Sammenlign monolitisk vs microservices TTM
- Vis feature flag impact

---

## Spørgsmål 12: Virtualisering

### Teoretisk indhold
- **VMs vs Containers**: Isolation level, resource overhead, startup time
- **Docker**: Containerization platform
- **Konsistent miljø**: Dev = Staging = Production
- **Fordele/ulemper**: Resource efficiency, portability, security

### Demonstrationer fra codebase
✅ **Kan demonstreres direkte:**

1. **Docker implementation**
   - **Filer**: Alle services har `Dockerfile`
   - **Fil**: `docker-compose.yml` - Orchestration
   - **Demonstration**:
     - Vis Dockerfile eksempler
     - Vis docker-compose setup
     - Vis hvordan hele systemet kører i containers
     - Vis konsistent miljø (samme image i dev og prod)

2. **Container orchestration**
   - **Fil**: `docker-compose.yml`
   - **Demonstration**: 
     - Vis service dependencies
     - Vis network isolation
     - Vis volume management

3. **Multi-stage builds** (hvis implementeret)
   - Check Dockerfiles for multi-stage builds
   - Vis optimization

### Konsistent miljø
**Demonstration:**
- **Dev**: `docker-compose up` → hele systemet kører lokalt
- **Prod**: Samme images → samme behavior
- **Benefit**: "Works on my machine" problem løst

### Fordele og ulemper
**Diskussion baseret på codebase:**

**Fordele:**
1. **Portability**: Kører på hvilket som helst system med Docker
2. **Isolation**: Hver service isoleret
3. **Resource efficiency**: Lavere overhead end VMs
4. **Fast startup**: Containers starter hurtigt
5. **Scalability**: Let at scale op/ned (se `replicas: 3`)

**Ulemper:**
1. **Security**: Container escape potential (mindre isoleret end VMs)
2. **Debugging**: Sværere at debug i containers
3. **State management**: Stateless design nødvendig
4. **Network complexity**: Container networking kan være kompleks

### Skalerbar applikation
**Diskussion:**
- **Hvordan hjælper Docker med skalering?**
  - Horizontal scaling (replicas)
  - Load distribution
  - Resource limits per container
  - Easy deployment af nye instanser

### Anbefaling
**Branch**: `demo/virtualization`
- Sammenlign VM vs Container overhead
- Vis container networking
- Dokumenter skalering benefits
- Vis security considerations

---

## Generelle anbefalinger

### Branches at oprette
1. `demo/async-design` - Async performance tests
2. `demo/fault-isolation` - Fallback patterns
3. `demo/scaling-cube` - Headroom calculations
4. `demo/green-architecture` - Resource metrics
5. `demo/devops-automation` - CI/CD pipeline
6. `demo/recovery` - Rollback mechanisms
7. `demo/specifications` - Formel specifikation eksempel
8. `demo/monitoring` - Monitoring demonstrations
9. `demo/organizational-scaling` - Risiko-analyse
10. `demo/availability` - Availability tests
11. `demo/time-to-market` - TTM metrics
12. `demo/virtualization` - Container comparisons

### Tools til demonstrationer
- **Load testing**: k6, Apache Bench, Postman
- **Monitoring**: Zipkin UI, Seq UI, Grafana
- **Container management**: Docker Desktop, docker-compose
- **Feature flags**: FeatureHub UI
- **CI/CD**: GitHub Actions (anbefalet)

### Præsentationsstruktur for hver spørgsmål
1. **Teoretisk introduktion** (2-3 min)
2. **Codebase demonstration** (5-7 min)
3. **Live demo** (hvis muligt) (3-5 min)
4. **Diskussion af kompromiser/valg** (2-3 min)
5. **Q&A** (resten af tiden)

### Noter
- Nogle spørgsmål (7, 9) er primært teoretiske men kan relateres til codebase
- Fokus på klare, visuelle demonstrationer
- Forbered backup slides hvis live demo fejler
- Dokumenter alle demonstrationer med screenshots/videoer

