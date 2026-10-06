import { suggestCities } from './city-suggestions';

const CITIES = ['Abatiá', 'Blumenau', 'Brusque', 'Balneário Camboriú', 'Camboriú', 'São José'];

describe('suggestCities', () => {
  it('põe primeiro as que começam com o texto e depois as que o contêm', () => {
    expect(suggestCities(CITIES, 'cambo').map((city) => city.name)).toEqual([
      'Camboriú',
      'Balneário Camboriú',
    ]);
  });

  it('ignora acentos e maiúsculas', () => {
    expect(suggestCities(CITIES, 'SAO JOSE').map((city) => city.name)).toEqual(['São José']);
    expect(suggestCities(CITIES, 'abatia')[0]).toEqual({
      name: 'Abatiá',
      before: '',
      match: 'Abatiá',
      after: '',
    });
  });

  it('separa o trecho encontrado para destacar', () => {
    expect(suggestCities(CITIES, 'camb')[1]).toEqual({
      name: 'Balneário Camboriú',
      before: 'Balneário ',
      match: 'Camb',
      after: 'oriú',
    });
  });

  it('sem texto, mostra todas', () => {
    expect(suggestCities(CITIES, '  ')).toHaveLength(CITIES.length);
  });
});
