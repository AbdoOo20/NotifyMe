using NotifyMe.Core.Services.Interface;
using PhoneNumbers;

namespace NotifyMe.Services.CommonService
{
    public class CommonService : ICommonService
    {
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            try
            {
                var phoneNumberUtil = PhoneNumberUtil.GetInstance();

                var number = phoneNumberUtil.Parse(phoneNumber, null);

                return phoneNumberUtil.IsValidNumber(number);
            }
            catch (NumberParseException)
            {
                return false;
            }
        }
    }
}
