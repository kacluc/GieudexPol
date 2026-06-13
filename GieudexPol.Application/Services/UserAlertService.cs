using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class UserAlertService : IUserAlertService
    {
        private readonly IUserAlertRepository _userAlertRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IRateSourceRepository _rateSourceRepository;

        public UserAlertService(
            IUserAlertRepository userAlertRepository,
            ICurrencyRepository currencyRepository,
            IRateSourceRepository rateSourceRepository)
        {
            _userAlertRepository = userAlertRepository;
            _currencyRepository = currencyRepository;
            _rateSourceRepository = rateSourceRepository;
        }

        public async Task<UserAlert?> GetByIdAsync(int id)
        {
            return await _userAlertRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<UserAlert>> GetAllAsync()
        {
            return await _userAlertRepository.GetAllAsync();
        }

        public async Task AddAsync(UserAlert entity)
        {
            await _userAlertRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(UserAlert entity)
        {
            await _userAlertRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(UserAlert entity)
        {
            await _userAlertRepository.DeleteAsync(entity);
        }

        public async Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId)
        {
            return await _userAlertRepository.GetUserAlertsByUserIdAsync(userId);
        }

        public async Task<IReadOnlyList<RateSource>> GetActiveRateSourcesAsync()
        {
            return await GetActiveRateSourcesAsync(includeTestSources: false);
        }

        public async Task<IReadOnlyList<RateSource>> GetActiveRateSourcesAsync(
            bool includeTestSources)
        {
            var sources = await _rateSourceRepository.GetActiveAsync();
            return includeTestSources
                ? sources
                : sources.Where(source => !IsTestRateSource(source.Code)).ToList();
        }

        public async Task CreateUserAlertAsync(UserAlert userAlert)
        {
            await CreateUserAlertAsync(userAlert, allowTestRateSources: false);
        }

        public async Task CreateUserAlertAsync(
            UserAlert userAlert,
            bool allowTestRateSources)
        {
            await ValidateAsync(userAlert, allowTestRateSources);
            userAlert.CreatedDate = System.DateTime.UtcNow;
            userAlert.Status = AlertStatus.Active;
            await _userAlertRepository.AddAsync(userAlert);
        }

        public async Task UpdateUserAlertAsync(UserAlert userAlert)
        {
            await UpdateUserAlertAsync(userAlert, allowTestRateSources: false);
        }

        public async Task UpdateUserAlertAsync(
            UserAlert userAlert,
            bool allowTestRateSources)
        {
            await ValidateAsync(userAlert, allowTestRateSources);
            await _userAlertRepository.UpdateAsync(userAlert);
        }

        public async Task DeleteUserAlertAsync(int userAlertId)
        {
            var userAlert = await _userAlertRepository.GetByIdAsync(userAlertId);
            if (userAlert != null)
            {
                await _userAlertRepository.DeleteAsync(userAlert);
            }
        }

        private async Task ValidateAsync(
            UserAlert userAlert,
            bool allowTestRateSources)
        {
            if (!Enum.IsDefined(userAlert.AlertType))
            {
                throw new ArgumentException("Nieprawidlowy typ alertu.");
            }

            if (!Enum.IsDefined(userAlert.PriceSide))
            {
                throw new ArgumentException("Nieprawidlowa monitorowana strona ceny.");
            }

            var currency = await _currencyRepository.GetByIdAsync(userAlert.CurrencyId);
            if (currency == null)
            {
                throw new ArgumentException("Wybrana waluta nie istnieje.");
            }

            if (userAlert.RateSourceId.HasValue)
            {
                var source = await _rateSourceRepository.GetByIdAsync(userAlert.RateSourceId.Value);
                if (source == null || !source.IsActive)
                {
                    throw new ArgumentException("Wybrane zrodlo kursu nie istnieje lub jest nieaktywne.");
                }

                if (!allowTestRateSources && IsTestRateSource(source.Code))
                {
                    throw new ArgumentException(
                        "Testowe zrodla kursow sa dostepne tylko dla administratorow.");
                }
            }

            if (userAlert.AlertType == AlertType.Threshold)
            {
                if (!userAlert.ThresholdValue.HasValue || userAlert.ThresholdValue <= 0)
                {
                    throw new ArgumentException("Alert progowy wymaga dodatniej wartosci progu.");
                }

                if (!userAlert.ThresholdDirection.HasValue ||
                    !Enum.IsDefined(userAlert.ThresholdDirection.Value))
                {
                    throw new ArgumentException("Alert progowy wymaga prawidlowego kierunku progu.");
                }

                userAlert.PercentageChange = null;
                userAlert.TimeFrameHours = null;
                return;
            }

            if (!userAlert.PercentageChange.HasValue || userAlert.PercentageChange <= 0)
            {
                throw new ArgumentException("Alert zmiany ceny wymaga dodatniej zmiany procentowej.");
            }

            if (userAlert.TimeFrameHours.HasValue && userAlert.TimeFrameHours <= 0)
            {
                throw new ArgumentException("Okres alertu musi byc dodatni.");
            }

            userAlert.ThresholdValue = null;
            userAlert.ThresholdDirection = null;
        }

        private static bool IsTestRateSource(string code)
        {
            return string.Equals(
                       code,
                       DevelopmentIdentity.RateSourceCode,
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       code,
                       DevelopmentIdentity.RateSourceCodeB,
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
