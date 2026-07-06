// Formatação de números como moeda brasileira (ex.: 1234.5 -> "R$ 1.234,50").
const formatoMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

export function formatarMoeda(valor: number): string {
  return formatoMoeda.format(valor);
}
