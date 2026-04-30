import { DashboardSummary } from '../core/models/dashboard.model';
import { TransactionHistoryEntry } from '../core/models/transaction-history.model';
import { FinTransaction } from '../core/models/transaction.model';

export const dashboardSummaryMock: DashboardSummary = {
  currentBalance: 1245.5,
  totalIncomeMonth: 3200,
  totalExpenseMonth: 1954.5,
  cardColor: 'green'
};

export const transactionsMock: FinTransaction[] = [
  {
    id: 'tx-001',
    userId: '11111111-1111-1111-1111-111111111111',
    categoryId: '22222222-2222-2222-2222-222222222221',
    categoryName: 'Salary',
    amount: 3200,
    transactionDateUtc: '2026-04-02T00:00:00Z',
    type: 'Income',
    description: 'Salario Abril',
    isDeleted: false,
    createdAtUtc: '2026-04-02T08:00:00Z',
    updatedAtUtc: null
  },
  {
    id: 'tx-002',
    userId: '11111111-1111-1111-1111-111111111111',
    categoryId: '22222222-2222-2222-2222-222222222222',
    categoryName: 'Bills',
    amount: 800,
    transactionDateUtc: '2026-04-05T00:00:00Z',
    type: 'Expense',
    description: 'Aluguel',
    isDeleted: false,
    createdAtUtc: '2026-04-05T08:00:00Z',
    updatedAtUtc: null
  }
];

export const historyMock: TransactionHistoryEntry[] = [
  {
    id: 'history-001',
    transactionId: 'tx-001',
    action: 'Created',
    description: 'Transaction created.',
    previousValues: null,
    currentValues: '{"amount":3200}',
    occurredAtUtc: '2026-04-02T08:00:00Z'
  }
];
