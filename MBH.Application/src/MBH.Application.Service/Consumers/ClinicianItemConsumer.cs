using System.Threading.Tasks;
using MassTransit;
using MBH.Common.Contracts;
using MBH.Common;
using MBH.Common.Entities;

namespace MBH.Application.Service.Consumers
{
    public class ClinicianItemConsumer : IConsumer<ClinicianItem>
    {
        private readonly IRepository<PatientInfo>repository;

        public ClinicianItemConsumer(IRepository<PatientInfo> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<ClinicianItem> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(item => item.PatientId == message.PatientId && item.LabTestId == message.LabTestId);

            if (item == null)
            {
                item = new PatientInfo
                {
                    LabTestId = message.LabTestId,
                    Name = message.Name,
                    Value = message.Value,
                    PatientId = message.PatientId,
                    Note = message.Note,
                    TestDate = message.TestDate,
                    TenantId = message.TenantId
                };

                await repository.CreateAsync(item);
            }
            else
            {
                item.Note = message.Note;
                item.Value = message.Value;
                item.TestDate = message.TestDate;
                await repository.UpdateAsync(item);
            }
        }

       
    }
}