export interface DashboardSummary {
  currentBalance: number;
  totalIncomeMonth: number;
  totalExpenseMonth: number;
  cardColor: 'red' | 'green';
}

export interface BalanceEvolutionPoint {
  label: string;
  balance: number;
  income: number;
  expense: number;
}
