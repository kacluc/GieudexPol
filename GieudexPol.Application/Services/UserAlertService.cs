using GieudexPol.Application.Interfaces;
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
            return await _rateSourceRepository.GetActiveAsync();
        }

        public async Task CreateUserAlertAsync(UserAlert userAlert)
        {
            await ValidateAsync(userAlert);
            userAlert.CreatedDate = System.DateTime.UtcNow;
            userAlert.Status = AlertStatus.Active;
            await _userAlertRepository.AddAsync(userAlert);
        }

        public async Task UpdateUserAlertAsync(UserAlert userAlert)
        {
            await ValidateAsync(userAlert);
            if (userAlert.Status != AlertStatus.Fulfilled)
            {
                userAlert.TriggeredDate = null;
            }
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

        private async Task ValidateAsync(UserAlert userAlert)
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
    }
}
