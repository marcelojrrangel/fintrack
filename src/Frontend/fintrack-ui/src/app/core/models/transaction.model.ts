export type TransactionType = 'Income' | 'Expense';

export interface FinTransaction {
  id: string;
  userId: string;
  categoryId: string;
  categoryName: string;
  amount: number;
  transactionDateUtc: string;
  type: TransactionType;
  description: string;
  isDeleted: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface TransactionMutationPayload {
  categoryId: string;
  amount: number;
  transactionDateUtc: string;
  type: TransactionType;
  description: string;
}

export interface TransactionFilter {
  description?: string;
  categoryId?: string;
  dateFrom?: string;
  dateTo?: string;
  type?: TransactionType | '';
  minAmount?: number;
  maxAmount?: number;
}
