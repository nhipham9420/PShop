namespace PShop.Utility
{
    public static class ValueStore
    {
        public const string RoleAdmin = "Admin";
        public const string RoleManager = "Manager";
        public const string RoleCustomer = "Customer";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusCompleted = "Completed";

        //public const string PaymentStatusRejected = "Rejected";

        public const string SessionCart = "SessionShoppingCart";
    }
}