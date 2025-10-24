# Desafio - Consulta de Endereços

Aplicação web que permite buscar endereços por **CEP** ou por **Estado e Cidade**, utilizando uma interface amigável e integração com uma **API em C# (.NET)** que consome a **API ViaCEP** e **BrasilAPI**.

---

## Como Executar o Projeto

### 1️⃣ Clonar o repositório
```bash
git clone https://github.com/gauvovic/desafio-almah.git
cd desafio-almah
```

---

### 2️⃣ Rodar o Backend (.NET)
```bash
cd api
dotnet run
```
A API será iniciada em:
http://localhost:5010

---

### 3️⃣ Rodar o Frontend
```bash
cd ../
npx serve . -p 5173
```
```
Ou abra o arquivo `index.html` direto no navegador.  
```
---

## Como Usar

### Busca por CEP
1. Digite um CEP válido (ex: `88350250`).  
2. Clique em **Buscar**.  
3. O endereço será exibido no painel à direita.

### Busca por Estado e Cidade
1. Selecione a **UF** e a **Cidade**.  
2. Digite o **Logradouro** (mínimo 3 caracteres).  
3. Clique em **Buscar**.  
4. Os resultados serão listados dinamicamente.
