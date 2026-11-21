using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;

namespace BookNeighbors.Api.Interfaces;

public interface ILoanService
{
    /// <summary>
    /// Restituisce la lista delle richieste di prestito inviate dall'utente specificato.
    /// Restituisce una lista vuota se l'utente non ha inviato richieste di prestito.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<LoanRequest>> GetSentAsync(Guid userId);
    /// <summary>
    /// Restituisce la lista delle richieste di prestito ricevute dall'utente specificato.
    /// Restituisce una lista vuota se l'utente non ha ricevuto richieste di prestito.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<LoanRequest>> GetReceivedAsync(Guid userId);
    /// <summary>
    /// Crea una nuova richiesta di prestito per il libro specificato, se l'utente è autenticato.
    /// Restituisce Unauthorized se l'utente non è autenticato.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>

    Task<LoanResult> CreateAsync(Guid bookId, Guid userId);
    /// <summary>
    /// Cambia lo stato della richiesta di prestito specificata, se l'utente è autenticato e autorizzato.       
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="action"></param>
    /// <returns></returns>

    Task<LoanResult> ChangeStatusAsync(Guid id, Guid userId, LoanAction action);
}