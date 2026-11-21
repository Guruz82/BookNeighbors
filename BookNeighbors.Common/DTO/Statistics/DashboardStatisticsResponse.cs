namespace BookNeighbors.DTO.Statistics;

public sealed record DashboardStatisticsResponse(
    int RegisteredUsers,
    int Libraries,
    int PublishedLibraries,
    int Books,
    int PublishedBooks,
    int LoanRequests,
    int ActiveLoans,
    int BookViews
);