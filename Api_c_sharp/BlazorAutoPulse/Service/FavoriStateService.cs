using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAutoPulse.Services
{
    /// <summary>
    /// Service centralisé pour gérer les favoris et leurs connexions SignalR
    /// Version Singleton compatible - utilise IServiceProvider pour résoudre les dépendances Scoped
    /// </summary>
    public class FavoriStateService : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISignalRService _signalRService;

        private HashSet<int> _favorisAnnonceIds = new();
        private int? _currentUserId;
        private bool _isInitialized = false;

        public event Action? OnFavorisChanged;

        public FavoriStateService(
            IServiceProvider serviceProvider,
            ISignalRService signalRService)
        {
            _serviceProvider = serviceProvider;
            _signalRService = signalRService;
        }

        /// <summary>
        /// Initialise le service avec l'ID utilisateur et charge ses favoris
        /// </summary>
        public async Task InitializeAsync(int userId)
        {
            if (_isInitialized && _currentUserId == userId)
            {
                Console.WriteLine("✓ FavoriStateService déjà initialisé");
                return;
            }

            _currentUserId = userId;
            Console.WriteLine($"🔄 Initialisation de FavoriStateService pour user {userId}...");

            try
            {
                // ✅ Créer un scope pour résoudre les services Scoped
                using var scope = _serviceProvider.CreateScope();
                var annonceService = scope.ServiceProvider.GetRequiredService<IAnnonceService>();

                // Charger tous les favoris de l'utilisateur
                var annoncesFavoris = await annonceService.GetAnnoncesFavoritesByCompteId(userId);
                _favorisAnnonceIds = annoncesFavoris.Select(a => a.IdAnnonce).ToHashSet();

                // Rejoindre tous les groupes SignalR
                foreach (var idAnnonce in _favorisAnnonceIds)
                {
                    await _signalRService.JoinFavorisAnnonce(idAnnonce);
                }

                _isInitialized = true;
                Console.WriteLine($"✅ {_favorisAnnonceIds.Count} favoris chargés et rejoints sur SignalR");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur initialisation FavoriStateService: {ex.Message}");
            }
        }

        /// <summary>
        /// Ajoute une annonce aux favoris et rejoint le groupe SignalR
        /// </summary>
        public async Task<bool> AddFavorisAsync(int idAnnonce)
        {
            if (!_currentUserId.HasValue)
            {
                Console.WriteLine("❌ Utilisateur non connecté");
                return false;
            }

            try
            {
                // ✅ Créer un scope pour résoudre les services Scoped
                using var scope = _serviceProvider.CreateScope();
                var favorisService = scope.ServiceProvider.GetRequiredService<IFavorisService>();

                // Ajouter en base de données
                var result = await favorisService.ToggleFavorite(_currentUserId.Value, idAnnonce);

                if (result && !_favorisAnnonceIds.Contains(idAnnonce))
                {
                    // Ajouter localement
                    _favorisAnnonceIds.Add(idAnnonce);

                    // Rejoindre le groupe SignalR
                    await _signalRService.JoinFavorisAnnonce(idAnnonce);

                    Console.WriteLine($"✅ Annonce {idAnnonce} ajoutée aux favoris et rejointe sur SignalR");
                    
                    OnFavorisChanged?.Invoke();
                    return true;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur ajout favori: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retire une annonce des favoris et quitte le groupe SignalR
        /// </summary>
        public async Task<bool> RemoveFavorisAsync(int idAnnonce)
        {
            if (!_currentUserId.HasValue)
            {
                Console.WriteLine("❌ Utilisateur non connecté");
                return false;
            }

            try
            {
                // ✅ Créer un scope pour résoudre les services Scoped
                using var scope = _serviceProvider.CreateScope();
                var favorisService = scope.ServiceProvider.GetRequiredService<IFavorisService>();

                // Retirer de la base de données
                var result = await favorisService.ToggleFavorite(_currentUserId.Value, idAnnonce);

                if (!result && _favorisAnnonceIds.Contains(idAnnonce))
                {
                    // Retirer localement
                    _favorisAnnonceIds.Remove(idAnnonce);

                    // Quitter le groupe SignalR
                    await _signalRService.LeaveFavorisAnnonce(idAnnonce);

                    Console.WriteLine($"✅ Annonce {idAnnonce} retirée des favoris et quittée sur SignalR");
                    
                    OnFavorisChanged?.Invoke();
                    return false;
                }

                return !result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur retrait favori: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Toggle le statut favori (ajoute ou retire selon l'état actuel)
        /// </summary>
        public async Task<bool> ToggleFavorisAsync(int idAnnonce)
        {
            bool isFavorite = _favorisAnnonceIds.Contains(idAnnonce);
            if (isFavorite)
            {
                return await RemoveFavorisAsync(idAnnonce);
            }

            return await AddFavorisAsync(idAnnonce);
        }

        /// <summary>
        /// Vérifie si une annonce est en favori
        /// </summary>
        public bool IsFavorite(int idAnnonce)
        {
            return _favorisAnnonceIds.Contains(idAnnonce);
        }

        /// <summary>
        /// Retourne la liste des IDs d'annonces en favoris
        /// </summary>
        public IReadOnlyCollection<int> GetFavorisIds()
        {
            return _favorisAnnonceIds.ToList().AsReadOnly();
        }

        /// <summary>
        /// Recharge tous les favoris depuis la base de données
        /// </summary>
        public async Task RefreshAsync()
        {
            if (!_currentUserId.HasValue) return;

            await InitializeAsync(_currentUserId.Value);
        }
        
        public async Task SetUser(int idUser)
        {
            _currentUserId = idUser;
        }

        public void Dispose()
        {
            _favorisAnnonceIds.Clear();
            _isInitialized = false;
            Console.WriteLine("🔌 FavoriStateService disposed");
        }
    }
}
