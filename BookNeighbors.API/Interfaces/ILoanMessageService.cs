using BookNeighbors.Api.Models.Results;
using BookNeighbors.DTO.LoanMessages;

namespace BookNeighbors.Api.Interfaces;
/// <summary>
/// Interfaccia per il servizio di gestione dei messaggi di prestito.
/// </summary>
public interface ILoanMessageService
{
    /// <summary>
    /// Restituisce i messaggi associati a una determinata richiesta di prestito, se l'utente è autorizzato a visualizzarli.
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<LoanMessageResponse>?> GetMessagesAsync(Guid loanId, Guid userId);
    /// <summary>
    /// Invia un messaggio relativo a una richiesta di prestito, se l'utente è autorizzato a farlo e la conversazione è aperta. 
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="senderId"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<LoanMessageSendResult> SendMessageAsync(Guid loanId, Guid senderId, string text);
    /// <summary>
    /// Restituisce l'ID del destinatario di un messaggio relativo a una richiesta di prestito, se l'utente è autorizzato a inviare messaggi per quella richiesta.
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="senderId"></param>
    /// <returns></returns>
    Task<Guid?> GetRecipientIdAsync(Guid loanId, Guid senderId);
}