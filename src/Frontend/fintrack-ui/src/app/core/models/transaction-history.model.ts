export interface TransactionHistoryEntry {
  id: string;
  transactionId: string;
  action: 'Created' | 'Updated' | 'Deleted' | 'Restored';
  description: string;
  previousValues: string | null;
  currentValues: string | null;
  occurredAtUtc: string;
}
