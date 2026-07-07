# AI Code Review Agent - Guía de uso y configuración

## 1. Descripción

AI Code Review Agent es una plataforma de revisión automática de código para Pull Requests usando .NET, GitHub Actions, OpenAI y agentes con herramientas.

El objetivo del proyecto no es solo llamar a un LLM, sino construir una arquitectura extensible preparada para:

* AI Agents
* GitHub Pull Request Review
* Context Engineering
* Tools
* MCP
* RAG sobre repositorios
* Múltiples agentes especializados
* Comparación futura entre modelos

## 2. Capacidades actuales

El sistema actualmente puede:

* Analizar Pull Requests en GitHub.
* Leer archivos modificados.
* Ejecutar `dotnet build`.
* Ejecutar `dotnet test`.
* Generar review por archivo usando IA.
* Calcular un Review Score.
* Generar resumen ejecutivo del PR.
* Publicar o actualizar un comentario único en el Pull Request.
* Usar tools locales como:

  * `read_file`
  * `search_text`
  * `run_build`
  * `run_tests`
  * `read_solution`
  * `read_project_file`
  * `find_class`
  * `find_interface`
* Usar un Agent Orchestrator.
* Usar planner heurístico o planner basado en LLM.
* Usar contexto RAG local.
* Ejecutar agentes especializados de seguridad y testing.
* Activar o desactivar capacidades desde `.ai-review.yml`.

## 3. Configuración del repositorio

Crear un archivo `.ai-review.yml` en la raíz del repositorio:

```yml
language: csharp
max_files: 10

features:
  llm_planner: true
  rag: true
  specialized_agents: true

rules:
  - Revisar Clean Architecture
  - Validar CancellationToken
  - Evitar Console.WriteLine
  - Revisar seguridad
  - Revisar testing
```

## 4. Opciones de configuración

### `language`

Define el lenguaje principal del proyecto.

Ejemplo:

```yml
language: csharp
```

### `max_files`

Define cuántos archivos modificados serán analizados como máximo.

Ejemplo:

```yml
max_files: 5
```

A menor número, menor consumo de tokens.

### `features.llm_planner`

Activa el planner basado en IA.

```yml
features:
  llm_planner: true
```

Cuando está activo, el agente usa IA para decidir qué tools ejecutar antes del review.

Para reducir costo:

```yml
features:
  llm_planner: false
```

### `features.rag`

Activa contexto adicional del repositorio.

```yml
features:
  rag: true
```

Permite que el agente busque archivos relacionados para mejorar el análisis.

Para reducir costo:

```yml
features:
  rag: false
```

### `features.specialized_agents`

Activa agentes especializados.

```yml
features:
  specialized_agents: true
```

Ejecuta agentes adicionales, por ejemplo:

* Security Agent
* Testing Agent

Esto mejora la calidad, pero aumenta el consumo porque genera llamadas extra al modelo.

Para reducir costo:

```yml
features:
  specialized_agents: false
```

## 5. Perfiles de uso recomendados

### Perfil económico

Ideal para repositorios personales o pruebas frecuentes.

```yml
language: csharp
max_files: 3

features:
  llm_planner: false
  rag: false
  specialized_agents: false

rules:
  - Revisar bugs potenciales
  - Revisar buenas prácticas .NET
```

### Perfil balanceado

Buena relación entre calidad y costo.

```yml
language: csharp
max_files: 5

features:
  llm_planner: false
  rag: true
  specialized_agents: false

rules:
  - Revisar Clean Architecture
  - Validar CancellationToken
  - Evitar Console.WriteLine
  - Revisar tests faltantes
```

### Perfil avanzado

Ideal para Pull Requests importantes.

```yml
language: csharp
max_files: 10

features:
  llm_planner: true
  rag: true
  specialized_agents: true

rules:
  - Revisar Clean Architecture
  - Validar CancellationToken
  - Evitar Console.WriteLine
  - Revisar seguridad
  - Revisar testing
  - Revisar mantenibilidad
```

## 6. Cómo controlar costos

Para reducir consumo:

1. Reducir `max_files`.
2. Desactivar `llm_planner`.
3. Desactivar `specialized_agents`.
4. Desactivar `rag` si el PR es pequeño.
5. Limitar el tamaño de outputs de tools con `AgentTextLimiter`.
6. Evitar analizar archivos sin patch.
7. Ejecutar build y tests una sola vez por PR.
8. Usar modelos económicos para reviews simples.

Orden recomendado de impacto en costo:

```text
specialized_agents: false
llm_planner: false
rag: false
max_files: 3
```

## 7. GitHub Actions

El proyecto se ejecuta desde GitHub Actions usando el comando:

```bash
dotnet run --project src/AiCodeReviewAgent.Cli -- analyze-pr
```

Variables requeridas:

```text
GITHUB_TOKEN
GITHUB_REPOSITORY
GITHUB_WORKSPACE
OPENAI_API_KEY
```

## 8. CLI local

Para analizar un repositorio local:

```bash
dotnet run --project src/AiCodeReviewAgent.Cli -- analyze "RUTA_REPOSITORIO"
```

Ejemplo:

```bash
dotnet run --project src/AiCodeReviewAgent.Cli -- analyze "D:\Projects\MyRepo"
```

## 9. Arquitectura general

Flujo actual:

```text
Program.cs
  ↓
PullRequestReviewWorkflow
  ↓
GitHubPullRequestClient
  ↓
RunBuildTool / RunTestsTool
  ↓
CodeReviewAgent
  ↓
AgentOrchestrator
  ↓
Planner
  ↓
ToolExecutor
  ↓
ToolProvider
  ↓
RAG Context
  ↓
Specialized Agents
  ↓
PromptBuilder
  ↓
OpenAI
  ↓
PullRequestSummaryAgent
  ↓
GitHubPullRequestCommentManager
```

## 10. Componentes principales

### `PullRequestReviewWorkflow`

Orquesta el flujo completo de revisión de Pull Request.

Responsabilidades:

* Cargar configuración.
* Leer archivos modificados.
* Ejecutar build.
* Ejecutar tests.
* Calcular score.
* Ejecutar reviews por archivo.
* Generar resumen ejecutivo.
* Publicar comentario en GitHub.

### `CodeReviewAgent`

Genera el review principal de cada archivo.

Usa:

* Agent Orchestrator
* RAG Context Builder
* Specialized Agents
* Prompt Builder
* OpenAI Client

### `AgentOrchestrator`

Coordina la ejecución de tools.

Flujo:

```text
Planner
  ↓
Execution Plan
  ↓
Tool Executor
  ↓
Tool Results
```

### `IAgentPlanner`

Define cómo se decide qué tools ejecutar.

Implementaciones:

* `HeuristicAgentPlanner`
* `LlmAgentPlanner`

### `IAgentToolProvider`

Abstracción preparada para MCP.

Permite que las tools puedan venir de:

* Tools locales
* MCP server
* Futuros proveedores externos

### `RepositoryRagContextBuilder`

Construye contexto adicional del repositorio para mejorar el review.

Actualmente usa búsqueda local simple. En el futuro puede reemplazarse por Supabase pgvector u otra base vectorial.

### `SpecializedReviewOrchestrator`

Ejecuta agentes especializados.

Agentes actuales:

* SecurityReviewAgent
* TestingReviewAgent

## 11. Review Score

El score se calcula considerando:

* Estado del build.
* Estado de tests.
* Cantidad de archivos revisados.

Ejemplo:

```text
100/100 → PR limpio
70/100  → build o tests fallan
50/100  → build y tests fallan
```

## 12. Publicación de comentario en GitHub

El comentario usa un marcador interno:

```html
<!-- ai-code-review-agent -->
```

Esto evita comentarios duplicados.

Si ya existe un comentario del agente, se actualiza.

Si no existe, se crea uno nuevo.

## 13. Recomendación de uso diario

Para trabajo normal:

```yml
language: csharp
max_files: 5

features:
  llm_planner: false
  rag: true
  specialized_agents: false
```

Para PR crítico:

```yml
language: csharp
max_files: 10

features:
  llm_planner: true
  rag: true
  specialized_agents: true
```

Para pruebas rápidas:

```yml
language: csharp
max_files: 2

features:
  llm_planner: false
  rag: false
  specialized_agents: false
```

## 14. Roadmap técnico

Próximas mejoras recomendadas:

1. Medición de tokens por ejecución.
2. Cache de resultados de tools.
3. Cache de RAG local.
4. Soporte real para MCP server.
5. Integración con Supabase pgvector.
6. Agentes especializados adicionales:

   * Architecture Agent
   * Performance Agent
   * Documentation Agent
7. Dashboard web.
8. Comparación entre GPT, Claude y Gemini.
9. Reportes históricos por repositorio.
10. Configuración por severidad del PR.

## 15. Filosofía del proyecto

Este proyecto busca crecer como una plataforma de ingeniería de producto, no como un script.

Principios:

* Separación clara de responsabilidades.
* Bajo acoplamiento.
* Extensibilidad.
* Control de costos.
* Observabilidad.
* Preparación para múltiples modelos.
* Preparación para MCP.
* Uso real en Pull Requests.
* Calidad suficiente para portafolio profesional y open source.
