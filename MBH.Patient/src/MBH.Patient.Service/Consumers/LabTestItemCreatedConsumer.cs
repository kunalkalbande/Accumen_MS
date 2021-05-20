using System.Threading.Tasks;
using MassTransit;
using MBH.LabTest.Contracts;
using MBH.Common;
using MBH.Patient.Service.Entities;

namespace MBH.Patient.Service.Consumers
{
    public class LabTestItemCreatedConsumer : IConsumer<LabTestCreated>
    {
        private readonly IRepository<PatientItem> repository;

        public LabTestItemCreatedConsumer(IRepository<PatientItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<LabTestCreated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(item => item.PatientId == message.PatientId && item.LabTestId == message.LabTestId);

            if (item != null)
            {
                return;
            }

            item = new PatientItem
            {
                LabTestId = message.LabTestId,
                Name = message.Name,
                Value = message.Value,
                PatientId=message.PatientId,
                Note=message.Note,
                TestDate=message.TestDate
            };

            await repository.CreateAsync(item);
        }

       
       
    }
}