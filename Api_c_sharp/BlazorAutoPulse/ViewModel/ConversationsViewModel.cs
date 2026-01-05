using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorAutoPulse.ViewModel;

public class ConversationViewModel : IDisposable
{
    private readonly ConversationStateService _conversationState;
    private readonly ISignalRService _signalR;
    private readonly IMessageService _messageService;
    private readonly IPieceJointeService _pieceJointeService;
    private readonly IBloqueService _bloqueService;
    private readonly IJSRuntime _jsRuntime;
    private readonly IOffreService _offreService;
    private readonly IAnnonceService _annonceService;

    public List<MessageDTO> Messages { get; private set; } = new();
    public ConversationListDTO? SelectedConversation { get; private set; }
    
    private string _newMessage = "";
    public string NewMessage 
    { 
        get => _newMessage;
        set => _newMessage = value; // ✅ AUCUN NotifyStateChanged
    }
    
    public bool IsTyping { get; private set; } = false;
    public ElementReference MessagesContainer;

    public List<IBrowserFile> SelectedFiles { get; set; } = new();
    public bool IsUploadingFiles { get; set; } = false;
    public bool IsLoadingMessages { get; private set; } = false;

    private System.Threading.Timer? _typingTimer;
    private bool _typingNotified = false;
    
    public event Action? _refreshUI;

    public List<ConversationListDTO> Conversations => _conversationState.Conversations;
    public int CurrentUserId => _conversationState.CurrentUserId;
    public bool IsLoading => _conversationState.IsLoading;
    public Dictionary<int, string> ImageSources => _conversationState.ImageSources;
    
    public bool EstBloquer { get; set; }
    public string BlocageType { get; set; }


    public bool ShowOffreMode { get; set; } = false;
    public decimal OffreAmount { get; set; } = 0;
    public string OffreError { get; set; } = "";
    public int? AnnonceIdForOffre { get; set; }
    public decimal? AnnoncePrixMax { get; set; }

    public string OffreInfoMessage { get; private set; } = "";
    public string OffreInfoClass { get; private set; } = "";

    public ConversationViewModel(
        ConversationStateService conversationState,
        ISignalRService signalR,
        IMessageService msgService,
        IPieceJointeService pieceJointeService,
        IBloqueService bloqueService,
        IJSRuntime jsRuntime,
        IOffreService offreService,
        IAnnonceService annonceService)
    {
        _conversationState = conversationState;
        _signalR = signalR;
        _messageService = msgService;
        _pieceJointeService = pieceJointeService;
        _bloqueService = bloqueService;
        _jsRuntime = jsRuntime;
        _offreService = offreService;
        _annonceService = annonceService;

        _signalR.OnMessageReceived += HandleMessageReceived;
        _signalR.OnUserTyping += HandleUserTyping;
        _signalR.OnMessagesRead += HandleMessagesRead;
        _conversationState.OnStateChanged += HandleGlobalStateChanged;
        _signalR.OnOffreStatusChanged += HandleOffreStatusChanged;
        _signalR.OnMessageWithOffreReceived += HandleMessageWithOffreReceived;
    }

    public async Task InitializeAsync()
    {
        await _conversationState.InitializeAsync();
    }

    public async Task SelectConversation(ConversationListDTO conv)
    {
        SelectedConversation = conv;
        await LoadMessages(conv.IdConversation);
        await ABloquer(true);
        NotifyStateChanged();
    }

    private async void HandleOffreStatusChanged(int idOffre, bool? estAccepte)
    {
        if (SelectedConversation == null) return;

        // Trouver le message contenant cette offre
        var message = Messages.FirstOrDefault(m =>
            m.Offres != null && m.Offres.Any(o => o.IdOffre == idOffre));

        if (message != null && message.Offres != null)
        {
            var offre = message.Offres.FirstOrDefault(o => o.IdOffre == idOffre);
            if (offre != null)
            {
                offre.EstAccepte = estAccepte;
                Console.WriteLine($"✅ Offre {idOffre} mise à jour en temps réel");
                NotifyStateChanged();
            }
        }
    }

    private async Task LoadMessages(int conversationId)
    {
        IsLoadingMessages = true;
        NotifyStateChanged();

        try
        {
            var conv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
            if (conv != null && conv.NombreNonLu > 0)
            {
                Console.WriteLine($"📭 Marquage de {conv.NombreNonLu} messages comme lus");
                conv.NombreNonLu = 0;
                _conversationState.NotifyMessagesRead();
            }

            Messages = (await _messageService.GetMessagesByConversationAndMarkAsRead(conversationId, CurrentUserId)).ToList();
            Console.WriteLine($"✅ {Messages.Count} messages chargés pour conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement messages: {ex.Message}");
        }
        finally
        {
            IsLoadingMessages = false;
            NotifyStateChanged();
        }
    }

    public void OnFilesSelected(List<IBrowserFile> files)
    {
        SelectedFiles = files;
        NotifyStateChanged();
    }

    private async void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        if (SelectedConversation?.IdConversation == conversationId)
        {
            // ✅ CORRECTION : Si c'est notre propre message, vérifier par ID exact
            if (senderId == CurrentUserId)
            {
                // Notre propre message : vérifier s'il existe déjà par contenu ET date
                var exists = Messages.Any(m => 
                    m.IdCompte == senderId && 
                    m.ContenuMessage == message && 
                    Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

                if (exists)
                {
                    Console.WriteLine($"⚠️ Message déjà présent localement (ignoré de SignalR)");
                    return;
                }
            }

            var newMsg = new MessageDTO
            {
                IdConversation = conversationId,
                IdCompte = senderId,
                ContenuMessage = message,
                DateEnvoiMessage = date,
                EstLu = senderId == CurrentUserId
            };

            // Pour les messages des autres, vérifier aussi
            var messageExists = Messages.Any(m => 
                m.IdCompte == senderId && 
                m.ContenuMessage == message && 
                Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

            if (!messageExists)
            {
                Messages.Add(newMsg);
                Console.WriteLine($"📨 Message reçu de {senderId}: {message.Substring(0, Math.Min(30, message.Length))}...");
                NotifyStateChanged();
            }
            else
            {
                Console.WriteLine($"⚠️ Message déjà présent (doublon SignalR évité)");
            }
        }
    }

    private void HandleMessagesRead(int conversationId, int userId)
    {
        if (userId == CurrentUserId && SelectedConversation?.IdConversation == conversationId)
        {
            foreach (var msg in Messages.Where(m => m.IdCompte != CurrentUserId))
            {
                msg.EstLu = true;
            }
            NotifyStateChanged();
        }
    }

    private void HandleUserTyping(int conversationId, int userId, string userName)
    {
        if (SelectedConversation?.IdConversation != conversationId || userId == CurrentUserId)
            return;

        IsTyping = true;
        NotifyStateChanged();

        Task.Delay(3000).ContinueWith(_ =>
        {
            IsTyping = false;
            NotifyStateChanged();
        });
    }

    private void HandleGlobalStateChanged()
    {
        NotifyStateChanged();
    }

    public void HandleTyping(KeyboardEventArgs e)
    {
        if (SelectedConversation == null)
            return;

        // ✅ Ne PAS notifier SignalR à chaque frappe
        _typingTimer?.Dispose();
        _typingTimer = new System.Threading.Timer(_ =>
        {
            _typingNotified = false;
        }, null, 2000, Timeout.Infinite);

        if (_typingNotified)
            return;

        _typingNotified = true;

        // ✅ Async sans await (fire-and-forget)
        _ = _signalR.NotifyTyping(SelectedConversation.IdConversation, CurrentUserId, "User");
    }

    public async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NewMessage))
        {
            await SendMessageWithOffre();
        }
    }

    private void NotifyStateChanged()
    {
        _refreshUI?.Invoke();
    }
    
    public void RemoveFile(IBrowserFile file)
    {
        SelectedFiles.Remove(file);
        NotifyStateChanged();
    }
    
    public async Task ABloquer(bool premierBloque)
    {
        EstBloquer = await _bloqueService.ABloque(CurrentUserId, SelectedConversation.IdParticipant, premierBloque);
        Console.WriteLine($"EstBloquer: {EstBloquer}, premierBloque: {premierBloque}");
    
        if (EstBloquer)
        {
            if (premierBloque)
            {
                BlocageType = "je suis bloquer";
            }
            else
            {
                BlocageType = "est bloquer";
            }
        }
        else
        {
            if (premierBloque)
            {
                EstBloquer = await _bloqueService.ABloque(CurrentUserId, SelectedConversation.IdParticipant, false);
            
                if (EstBloquer)
                {
                    BlocageType = "est bloquer";
                }
                else
                {
                    BlocageType = "";
                }
            }
            else
            {
                BlocageType = "";
            }
        }

        _refreshUI?.Invoke();
    }
    
    public void OnInputChanged(ChangeEventArgs e)
    {
        NewMessage = e.Value?.ToString() ?? "";
    }

    public void Dispose()
    {
        _signalR.OnMessageReceived -= HandleMessageReceived;
        _signalR.OnUserTyping -= HandleUserTyping;
        _signalR.OnMessagesRead -= HandleMessagesRead;
        _conversationState.OnStateChanged -= HandleGlobalStateChanged;
        _signalR.OnOffreStatusChanged -= HandleOffreStatusChanged;
        _signalR.OnMessageWithOffreReceived -= HandleMessageWithOffreReceived;
        _typingTimer?.Dispose();
    }


    //OFFRES

    public async Task ToggleOffreMode()
    {
        ShowOffreMode = !ShowOffreMode;

        if (ShowOffreMode && SelectedConversation != null)
        {
            try
            {
                var annonce = await _annonceService.GetAnnonceDetailById(SelectedConversation.IdAnnonce);
                if (annonce != null)
                {
                    AnnonceIdForOffre = annonce.IdAnnonce;
                    AnnoncePrixMax = annonce.Prix;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement annonce: {ex.Message}");
            }
        }
        else
        {
            OffreAmount = 0;
            OffreError = "";
            OffreInfoMessage = ""; // Reset du message
        }

        NotifyStateChanged();
    }

    public void UpdateOffreAmount(string value)
    {
        if (decimal.TryParse(value, out var amount))
        {
            OffreAmount = amount;
            OffreError = "";

            if (AnnoncePrixMax.HasValue && AnnoncePrixMax.Value > 0)
            {
                var diff = AnnoncePrixMax.Value - OffreAmount;
                var percentage = (Math.Abs(diff) / AnnoncePrixMax.Value) * 100;

                if (diff > 0)
                {
                    // Prix inférieur à l'annonce (Réduction)
                    OffreInfoMessage = $"Réduction de {diff:N0} € (-{percentage:F1}%)";
                    OffreInfoClass = "offre-reduction"; // Classe CSS pour vert/positif
                }
                else if (diff < 0)
                {
                    OffreInfoMessage = $"Augmentation de {Math.Abs(diff):N0} € (+{percentage:F1}%)";
                    OffreInfoClass = "offre-increase";
                }
                else
                {
                    OffreInfoMessage = "Prix identique à l'annonce";
                    OffreInfoClass = "offre-neutral";
                }
            }
        }
        else
        {
            OffreError = "Montant invalide";
            OffreInfoMessage = "";
        }
        NotifyStateChanged(); // Important pour rafraîchir l'UI immédiatement
    }


    public async Task SendMessageWithOffre()
    {
        if (SelectedConversation == null)
            return;

        // ✅ Autoriser l'envoi si : message texte OU offre OU fichiers
        bool hasContent = !string.IsNullOrWhiteSpace(NewMessage) ||
                          (ShowOffreMode && OffreAmount > 0) ||
                          SelectedFiles.Any();

        if (!hasContent)
            return;

        // ✅ Valider l'offre si le mode offre est activé
        if (ShowOffreMode && OffreAmount <= 0)
        {
            OffreError = "Le montant doit être supérieur à 0";
            NotifyStateChanged();
            return;
        }

        // ✅ Sauvegarder les valeurs avant reset
        var messageText = string.IsNullOrWhiteSpace(NewMessage)
            ? (ShowOffreMode ? $"💰 Offre de {OffreAmount:N0} €" : "[Fichier(s) joint(s)]")
            : NewMessage.Trim();

        var filesToUpload = new List<IBrowserFile>(SelectedFiles);
        var offreAmountToSend = ShowOffreMode ? OffreAmount : 0;
        var annonceIdToSend = AnnonceIdForOffre;

        // ✅ Reset immédiat
        _newMessage = "";
        OffreAmount = 0;
        OffreError = "";
        OffreInfoMessage = "";
        ShowOffreMode = false;
        SelectedFiles.Clear();
        NotifyStateChanged();

        try
        {
            IsUploadingFiles = true;

            // ✅ Créer le message texte
            var messageDto = new MessageCreateDTO
            {
                IdConversation = SelectedConversation.IdConversation,
                IdCompte = CurrentUserId,
                ContenuMessage = messageText,
            };

            var createdMessage = await _messageService.CreateMessageAsync(messageDto, offreAmountToSend > 0 && annonceIdToSend.HasValue);

            if (createdMessage == null)
            {
                Console.WriteLine("❌ Erreur : message non créé");
                _newMessage = messageText;
                NotifyStateChanged();
                return;
            }

            // ✅ Créer l'offre LIÉE au message (SI mode offre était activé)
            if (offreAmountToSend > 0 && annonceIdToSend.HasValue)
            {
                try
                {
                    var offreDto = new OffreCreateDTO
                    {
                        IdAnnonce = annonceIdToSend.Value,
                        IdMessage = createdMessage.IdMessage,
                        Valeur = offreAmountToSend
                    };

                    // ✅ Récupérer directement l'offre créée
                    var offreCreee = await _offreService.CreateAsync(offreDto);

                    if (offreCreee != null)
                    {
                        Console.WriteLine($"✅ Offre {offreCreee.IdOffre} de {offreAmountToSend:N0} € créée");

                        await _signalR.SendMessageWithOffre(
                            SelectedConversation.IdConversation,
                            CurrentUserId,
                            messageText,
                            createdMessage.IdMessage,
                            offreCreee.IdOffre,  // ✅ Utiliser l'IdOffre retourné
                            offreAmountToSend,
                            annonceIdToSend.Value
                        );

                        // ✅ Recharger pour afficher l'offre via le composant
                        await LoadMessages(SelectedConversation.IdConversation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur création offre: {ex.Message}");
                }
            }
            else
            {
                await _signalR.SendMessage(
                    SelectedConversation.IdConversation,
                    CurrentUserId,
                    messageText
                );
            }

            // Upload des fichiers en arrière-plan
            if (filesToUpload.Any())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var uploadedFiles = await _pieceJointeService.UploadFilesAsync(
                            createdMessage.IdMessage,
                            filesToUpload);

                        createdMessage.PiecesJointes = uploadedFiles;
                        await Task.Delay(200);
                        NotifyStateChanged();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erreur upload: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur envoi message: {ex.Message}");
            _newMessage = messageText;
        }
        finally
        {
            IsUploadingFiles = false;
            NotifyStateChanged();
        }
    }

    public async Task AccepterOffre(int idOffre)
    {
        try
        {
            var success = await _offreService.AccepterOffreAsync(idOffre);
            if (success)
            {
                Console.WriteLine($"✅ Offre {idOffre} acceptée");
                if (SelectedConversation != null)
                {
                    await LoadMessages(SelectedConversation.IdConversation);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur acceptation offre: {ex.Message}");
        }
    }

    public async Task RefuserOffre(int idOffre)
    {
        try
        {
            var success = await _offreService.RefuserOffreAsync(idOffre);
            if (success)
            {
                Console.WriteLine($"✅ Offre {idOffre} refusée");
                // Recharger les messages
                if (SelectedConversation != null)
                {
                    await LoadMessages(SelectedConversation.IdConversation);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur refus offre: {ex.Message}");
        }
    }
    private async void HandleMessageWithOffreReceived(
    int conversationId,
    int senderId,
    string message,
    DateTime date,
    int idMessage,
    int idOffre,  // ✅ Recevoir l'IdOffre
    decimal offreValeur,
    int idAnnonce)
    {
        if (SelectedConversation?.IdConversation == conversationId)
        {
            // Vérifier si le message existe déjà
            var exists = Messages.Any(m =>
                m.IdCompte == senderId &&
                m.ContenuMessage == message &&
                Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

            if (!exists)
            {
                // Créer le message avec l'offre
                var newMsg = new MessageDTO
                {
                    IdMessage = idMessage,
                    IdConversation = conversationId,
                    IdCompte = senderId,
                    ContenuMessage = message,
                    DateEnvoiMessage = date,
                    EstLu = senderId == CurrentUserId,
                    Offres = new List<OffreDTO>
                {
                    new OffreDTO
                    {
                        IdOffre = idOffre,  // ✅ Utiliser l'IdOffre reçu
                        IdMessage = idMessage,
                        Valeur = offreValeur,
                        IdAnnonce = idAnnonce,
                        DateOffre = date,
                        EstAccepte = null
                    }
                }
                };

                Messages.Add(newMsg);
                Console.WriteLine($"✅ Message avec offre de {offreValeur}€ (IdOffre={idOffre}) ajouté");
                NotifyStateChanged();
            }
        }
    }
}