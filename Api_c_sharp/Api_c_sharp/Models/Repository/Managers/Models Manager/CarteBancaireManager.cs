using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using System.Security.Cryptography;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{

    public class CartebancaireManager : WriteableReadableManager<CarteBancaire>, ICarteBancaireRepository
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("CLE_SUPER_SECRETE_32_OCTETS!!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("INIT_VECTOR_16B!");

        public CartebancaireManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<CarteBancaire>> GetAllAsync()
        {
            var cartes = await dbSet.ToListAsync();
            foreach (var carte in cartes)
            {
                carte.NumeroCarte = MaskLast4Digits(carte.NumeroCarte);
            }
            return cartes;
        }

        public override async Task<CarteBancaire?> GetByIdAsync(int id)
        {
            var carte = await base.GetByIdAsync(id);
            if (carte != null)
            {
                carte.NumeroCarte = MaskLast4Digits(carte.NumeroCarte);
            }
            return carte;
        }

        public override Task<CarteBancaire> AddAsync(CarteBancaire entity)
        {
            entity.NumeroCarte = EncryptCardNumber(entity.NumeroCarte);
            return base.AddAsync(entity);
        }

        public async Task<IEnumerable<CarteBancaire>> GetCarteBancaireByCompteId(int compteid)
        {
            return await dbSet.Where(c => c.IdCompte == compteid).ToListAsync();
        }

        private string EncryptCardNumber(string numeroCarte)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(numeroCarte);

            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        private string DecryptCardNumber(string encryptedCard)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(encryptedCard);

            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        private string MaskLast4Digits(string encryptedCard)
        {
            var numeroCarte = DecryptCardNumber(encryptedCard);

            if (numeroCarte.Length < 3)
                throw new ArgumentException("Numéro invalide");

            return new string('*', numeroCarte.Length - 4) + numeroCarte[^4..];
        }
    }
}
