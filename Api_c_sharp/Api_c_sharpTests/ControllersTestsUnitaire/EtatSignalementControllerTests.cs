using Api_c_sharp.Mapper;
using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;
using AutoPulse.Shared.DTO;
using Api_c_sharp.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.ControllersUnitaires.Tests
{
    [TestClass()]
    public class EtatSignalementControllerTests
    {
        private EtatSignalementController _controller;
        private AutoPulseBdContext _context;
        private EtatSignalementManager _manager;
        private IMapper _mapper;
        private EtatSignalementPlainte _objetcommun;

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<AutoPulseBdContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AutoPulseBdContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            });
            _mapper = config.CreateMapper();

            _manager = new EtatSignalementManager(_context);
            _controller = new EtatSignalementController(_manager, _mapper);

            _context.EtatSignalementsPlaintes.RemoveRange(_context.EtatSignalementsPlaintes);
            await _context.SaveChangesAsync();

            var objet = new EtatSignalementPlainte()
            {
                LibelleEtatSignalement = "Test"
            };

            await _context.EtatSignalementsPlaintes.AddAsync(objet);
            await _context.SaveChangesAsync();

            _objetcommun = objet;
        }

        #region GET

            #region GetById
        [TestMethod]
        public async Task GetByIdTest()
        {
            // Act
            var result = await _controller.GetById(_objetcommun.IdEtatSignalement);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(EtatSignalementDTO));
            Assert.AreEqual(_objetcommun.LibelleEtatSignalement, result.Value.LibelleEtatSignalement);
        }

        [TestMethod]
        public async Task NotFoundGetByIdTest()
        {
            // Act
            var result = await _controller.GetById(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        #endregion

            #region GetAll
        [TestMethod]
        public async Task GetAllTest()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<EtatSignalementDTO>));
            Assert.IsTrue(result.Value.Any());
            Assert.IsTrue(result.Value.Any(o => o.LibelleEtatSignalement == _objetcommun.LibelleEtatSignalement));
        }
        #endregion 

        #endregion
    }
}