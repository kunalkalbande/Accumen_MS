using System.Threading.Tasks;
using MassTransit;
using MBH.LabTest.Contracts;
using MBH.Common;
using MBH.Patient.Service.Entities;

namespace MBH.Patient.Service.Consumers
{
    public class LabTestItemUpdatedConsumer : IConsumer<LabTestUpdated>
    {
        private readonly IRepository<PatientItem> repository;

        public LabTestItemUpdatedConsumer(IRepository<PatientItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<LabTestUpdated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(item => message.PatientId == message.PatientId && message.LabTestId == message.LabTestId);


            if (item == null)
            {
                item = new PatientItem
                {
                    PatientId=message.PatientId,
                    LabTestId = message.LabTestId,
                    Name ="",
                    Value = message.Value,
                    Note=message.Note,
                    TestDate=message.TestDate
                };

                await repository.CreateAsync(item);
            }
            else
            {
                item.Note = message.Note;
                item.Value=message.Value;
                item.TestDate=message.TestDate;
                await repository.UpdateAsync(item);
            }
        }
    }
}