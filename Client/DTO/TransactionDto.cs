namespace Client.DTO
{
    public class TransactionDto
    {
        public class CreateTransactionRequest
        {
            public string Description { get; set; }
            public string Category { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }

        public class EditTransactionRequest : CreateTransactionRequest
        {
            public int Id { get; set; }
        }

        public class DeleteTransactionRequest
        {
            public int Id { get; set; }
        }

        public class TransactionResponse
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }

    }
}
