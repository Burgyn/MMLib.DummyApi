# TODO - MMLib.DummyApi

## Refaktoring a vylepšenia

### 1. Odstránenie Orders a Products - migrácia na Custom Collections
**Cieľ:** Zbaviť sa pevne definovaných domén (Products, Orders) a namiesto toho rozumnejšie prerobiť Custom Collections tak, aby bolo možné definovať kolekcie s ich vlastnosťami (autentifikácia, background jobs, validácia) priamo pri vytváraní.

**Úlohy:**
- [ ] Odstrániť `Domain/Products` a `Domain/Orders` moduly
- [ ] Rozšíriť Custom Collections o možnosť definovať:
  - [ ] Autentifikáciu na úrovni kolekcie (namiesto pevne definovaných Orders)
  - [ ] Predvolené background job konfigurácie pri vytváraní kolekcie
  - [ ] Predvolené schémy validácie
  - [ ] Možnosť definovať "presets" alebo "templates" kolekcií (napr. `products`, `orders`)
- [ ] Upraviť `ResetEndpoint` - odstrániť referencie na Products/Orders
- [ ] Aktualizovať `DummyApiOptions` - odstrániť `InitialProductCount`, `InitialOrderCount`
- [ ] Aktualizovať dokumentáciu v README.md
- [ ] Aktualizovať `.http` súbory - odstrániť príklady Products/Orders

**Výhody:**
- Jednoduchšia architektúra - jeden mechanizmus pre všetko
- Flexibilnejšie - používateľ si vytvorí presne to, čo potrebuje
- Menej kódu na udržiavanie
- Lepšie demonštruje silu Custom Collections

---

## Sumár projektu

### Čo je MMLib.DummyApi?

**MMLib.DummyApi** je dummy REST API navrhnuté pre integračné testovanie a demonštráciu nástrojov na benchmarkovanie. Poskytuje realistické scenáre pre testovanie aplikácií bez potreby skutočnej databázy alebo externých služieb.

### Hlavný zmysel projektu

1. **Integračné testovanie (TeaPie)**
   - Testovanie retry logiky pomocou `X-Simulate-Retry` hlavičky
   - Testovanie error handlingu s `X-Simulate-Error`
   - Testovanie asynchrónnych operácií s background jobami
   - Testovanie autentifikačných flow s konfigurovateľnou autentifikáciou

2. **Benchmark nástroje**
   - Testovanie veľkosti payloadov (`/perf/payload`)
   - Testovanie konkurentných operácií (`/perf/counter`)
   - Simulácia latencie (`X-Simulate-Delay`)
   - Testovanie failure scenárov (`X-Chaos-FailureRate`)

3. **Vývoj a demonštrácia**
   - Rýchle prototypovanie API integrácií
   - Demonštrácia správania aplikácií pri rôznych scenároch
   - Vzdelávacie účely - ukážka Minimal API, vertical slice architektúry

### Kľúčové vlastnosti

- **Dynamické Custom Collections**: Vytváranie vlastných endpointov s ľubovoľnou JSON štruktúrou
- **Simulačné hlavičky**: Delay, retry, error, chaos - aplikovateľné na akýkoľvek endpoint
- **Background jobs**: Simulácia asynchrónneho spracovania s konfigurovateľnými operáciami
- **JSON Schema validácia**: Voliteľná validácia pre custom collections
- **Performance endpoints**: Generovanie payloadov, thread-safe counter
- **Docker podpora**: Natívna ASP.NET Core containerizácia
- **OpenAPI dokumentácia**: Scalar API Reference pre jednoduchú dokumentáciu

### Architektúra

- **Minimal API**: Moderný prístup k API vývoju v ASP.NET Core
- **Vertical Slice Architecture**: Organizácia podľa domén/features namiesto technických vrstiev
- **In-memory storage**: 
  - `ConcurrentDictionary` pre Products/Orders (do odstránenia)
  - `LiteDB` (in-memory) pre Custom Collections
- **Dependency Injection**: ASP.NET Core built-in DI
- **Middleware**: Custom middleware pre simuláciu sieťových podmienok

### Budúcnosť projektu

Po refaktoringu (odstránenie Products/Orders) bude projekt:
- Jednoduchší na údržbu
- Flexibilnejší pre používateľov
- Lepšie demonštrovať silu Custom Collections
- Vhodný ako verejný Docker image pre komunitu
