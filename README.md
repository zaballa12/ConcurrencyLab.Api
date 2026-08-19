# ConcurrencyLab.Api

Pequena API em ASP.NET Core para estudar concorrência (operações de estoque com EF Core e SQLite).

A ideia do projeto é mostrar um problema bem comum: duas requisições tentando alterar o mesmo produto ao mesmo tempo. Em uma implementação ingênua, isso pode gerar sobrescrita silenciosa de dados e deixar o estoque final diferente do esperado.

Neste projeto, a correcao foi feita com optimistic concurrency usando o campo `Version` na entidade `Product`.

```csharp
public Guid Version { get; set; }
```

Esse campo foi configurado como concurrency token no EF Core. Assim, quando o produto é atualizado, o EF compara a versão que foi lida no início da requisição com a versão atual da linha no banco. Se outra requisicao ja tiver salvo antes, essa versao nao bate mais, o `SaveChangesAsync()` falha com `DbUpdateConcurrencyException` e a API responde `409 Conflict`.

O projeto manteve dois endpoints de reserva:

- `POST /products/{id}/reserve-naive`
- `POST /products/{id}/reserve`

O primeiro existe como demonstração do problema. O segundo mostra a abordagem corrigida usando `Version` para detectar conflito entre atualizações concorrentes.
