using System.Runtime.Serialization;

namespace FP.Core.Api.Helpers
{
	public enum WithdrawStatusEnum
	{
		[EnumMember(Value = "Waiting")]
		Waiting,
		[EnumMember(Value = "Realized")]
		Realized,
		[EnumMember(Value = "Rejected")]
		Rejected,

	}
}
