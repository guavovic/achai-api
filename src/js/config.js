// Aberto do disco (file://) ou de localhost, o front chama a API local; publicado, chama a do Render.
const API_BASE_URL = ["", "localhost", "127.0.0.1"].includes(window.location.hostname)
  ? "http://localhost:5010"
  : "https://address-lookup-api.onrender.com";

const ESTADOS = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
  "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
  "RS", "RO", "RR", "SC", "SP", "SE", "TO"
];
