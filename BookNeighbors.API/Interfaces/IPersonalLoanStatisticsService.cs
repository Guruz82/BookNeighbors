
using BookNeighbors.DTO.Statistics;

namespace BookNeighbors.Api.Interfaces;

public interface IPersonalLoanStatisticsService
{
    Task<PersonalLoanStatisticsResponse> GetAsync(Guid userId);
}
