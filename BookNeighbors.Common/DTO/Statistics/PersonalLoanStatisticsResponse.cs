
namespace BookNeighbors.DTO.Statistics;

public record PersonalLoanStatisticsResponse(int LentBooks, int BorrowedBooks, int ActiveLoans, int CompletedLoans, List<PersonalLoanMonthResponse> Months);

public record PersonalLoanMonthResponse(string Month, int SentRequests, int ReceivedRequests);
