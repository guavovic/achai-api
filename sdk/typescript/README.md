# achai-api

Cliente TypeScript da [achaí-API](https://achai-api.vercel.app), com os tipos gerados a partir do documento OpenAPI.

```ts
import { createAchaiClient } from 'achai-api';

const achai = createAchaiClient();

const { data } = await achai.GET('/buscar/{cep}', { params: { path: { cep: '01001000' } } });
console.log(data?.enderecoFormatado);
```

Rotas, parâmetros e respostas: [achai-api.vercel.app/docs](https://achai-api.vercel.app/docs).
