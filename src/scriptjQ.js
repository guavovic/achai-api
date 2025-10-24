const API_BASE_URL = "http://localhost:5010";

const estados = [
  "AC", "AL", "AP", "AM", "BA",  "CE", "DF", "ES", "GO", "MA",
      "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
  "RS", "RO", "RR", "SC", "SP", "SE", "TO"
];

let cacheCidades = {};

$(document).ready(function () {
  const resultadoArea = $("#areaResultado");

  async function requestApi(url) {
    try {
      const resposta = await fetch(url);

      // if (!resposta.ok) tirar pra debugar depois
      //   throw new Error(`${resposta.status} - ${resposta.statusText}`);

      return await resposta.json();
    } catch (erro) {
      throw erro;
    }
  }

  function exibirLoad() {
    // resultadoArea.addClass("mostrar");
    resultadoArea.html(`<div class="loader"></div>`);
  }

  function carregarEnderecos(data) {
    // resultadoArea.addClass("mostrar");
    resultadoArea.html("");

    if (!data || data.length === 0) {
      resultadoArea.text("Nenhum resultado encontrado.");
      return;
    }

    const lista = Array.isArray(data) ? data : [data];
    lista.forEach(endereco => {
      resultadoArea.append(`
        <div class="endereco-item">
          <strong>CEP:</strong> ${endereco.cep}<br>
          <strong>Logradouro:</strong> ${endereco.logradouro}<br>
          <strong>Bairro:</strong> ${endereco.bairro}<br>
          <strong>Localidade:</strong> ${endereco.localidade}<br>
          <strong>UF:</strong> ${endereco.uf}<br>
          <strong>Estado:</strong> ${endereco.estado}<br>
          <strong>Região:</strong> ${endereco.regiao}
        </div>
      `);
    });

    resultadoArea.scrollTop(resultadoArea[0].scrollHeight);
  }

  // autocomplete UF
  $("#campoUf").autocomplete({
    source: estados,
    minLength: 0,
    select: function (event, ui) {
      loadCidades(ui.item.value);
    }
  }).focus(function () { $(this).autocomplete("search", ""); });

  // carregar cidades
  async function loadCidades(uf) {
    if (cacheCidades[uf]) {
      configurarAutocompleteCidade(cacheCidades[uf]);
      return;
    }

    try {
      exibirLoad();
      const cidades = await requestApi(`${API_BASE_URL}/buscar/cidades/${uf}`);
      const nomes = cidades.map(c => c.nome);
      cacheCidades[uf] = nomes;
      configurarAutocompleteCidade(nomes);
      resultadoArea.html("");
    } catch (erro) {
      resultadoArea.html(`<p style="color:red;">Erro ao carregar cidades de ${uf}: ${erro}</p>`);
    }
  }

  function configurarAutocompleteCidade(cidades) {
    $("#campoCidade").val("");
    $("#campoCidade").autocomplete({
      source: cidades,
      minLength: 0
    }).focus(function () { $(this).autocomplete("search", ""); });
  }

  // form de CEP
  $("#formBuscaCep").on("submit", async function (e) {
    e.preventDefault();
    const cep = $("#campoCep").val().trim();
    resultadoArea.html("");

    if (!/^\d{8}$/.test(cep)) { // pega tudo que n for digito e verifica se tem 8 digitos (testar depois)
      resultadoArea.html("<p style='color:red;'>Digite um CEP válido com 8 dígitos.</p>");
      return;
    }

    try {
      exibirLoad();
      const dados = await requestApi(`${API_BASE_URL}/buscar/${cep}`);
      carregarEnderecos(dados);
    } catch (erro) {
      resultadoArea.html(`<p style="color:red;">CEP não encontrado ou erro: ${erro}</p>`);
    }
  });

  // form de estrado/cidade/logradouro
  $("#formBuscaLocal").on("submit", async function (e) {
    e.preventDefault();
    const uf = $("#campoUf").val().trim().toUpperCase();
    const cidade = $("#campoCidade").val().trim();
    const logradouro = $("#campoLogradouro").val().trim();

    resultadoArea.html("");

    if (!uf || !cidade || !logradouro) {
      resultadoArea.html("<p style='color:red;'>Preencha todos os campos</p>");
      return;
    }

    if (cidade.length < 3 || logradouro.length < 3) {
      resultadoArea.html("<p style='color:red;'>Cidade e logradouro devem ter pelo menos 3 caracteres</p>");
      return;
    }

    try {
      exibirLoad();
      const dados = await requestApi(`${API_BASE_URL}/buscar/${uf}/${cidade}/${logradouro}`);
      carregarEnderecos(dados);
    } catch (erro) {
      resultadoArea.html(`<p style="color:red;">Erro ao buscar endereços: ${erro}</p>`);
    }
  });
});
