namespace Mynt.Core.Enums
{
	public enum OrderStatus
	{
		New = 0,
		PartiallyFilled = 1,
		Filled = 2,
		Canceled = 3,
		PendingCancel = 4,
		Rejected = 5,
		Expired = 6,
		Error = 7,
		Unknown = 8
	}
}
