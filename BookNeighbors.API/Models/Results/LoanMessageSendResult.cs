using BookNeighbors.Api.Enum;
using BookNeighbors.DTO.LoanMessages;

namespace BookNeighbors.Api.Models.Results;

public record LoanMessageSendResult(LoanMessageResponse? Message, LoanMessageError Error);