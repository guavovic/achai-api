# Achai.Client

Cliente .NET da [achaí-API](https://achai-api.vercel.app), gerado a partir do documento OpenAPI.

```csharp
var achai = AchaiClient.Create();

var endereco = await achai.Buscar["01001000"].GetAsync();
Console.WriteLine(endereco?.EnderecoFormatado);
```

Rotas, parâmetros e respostas: [achai-api.vercel.app/docs](https://achai-api.vercel.app/docs).
