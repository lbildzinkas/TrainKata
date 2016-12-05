namespace TrainKata.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Ploeh.AutoFixture;
    using System.Linq;

    [TestClass]
    public class Test
    {
        private ReservationService sut;

        private Mock<ITrainRepository> trainRepository;

        // an passager should be able to reserve an seat - OK
        // train should have < 70% seats reserved
        // coach should ideally(not a must) have < 70% seats reserved
        // tickets from the same order must be in the same coach, it means, some coaches may have > 70%, but you must keep the train < 70%

        [TestInitialize]
        public void Setup()
        {
            this.trainRepository = new Mock<ITrainRepository>();
            this.sut = new ReservationService(this.trainRepository.Object);
        }

        [TestMethod]
        public void CustomerShouldBeAbleToReserveSeat()
        {
            this.trainRepository.Setup(t => t.FindTrain(It.IsAny<int>())).Returns(() => new Train()
            {
                Capacity = 1,
                Coaches = new List<Coach>() {
                    new Coach ()
                    {
                    Lotation = 1
                    }
                }
            });
            var customerId = 1;
            var trainId = 1;
            var qty = 1;
            sut.ReserveSeat(customerId, trainId, qty);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TrainWithHighCapacityMustNotAllowReservation()
        {
            this.trainRepository.Setup(t => t.FindTrain(It.IsAny<int>())).Returns(() => new Train() { Capacity = 71 });
            var customerId = 1;
            var trainId = 1;
            var qty = 1;
            sut.ReserveSeat(customerId, trainId, qty);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TrainWithLowCapacityAndCoachedWithHighCapacityMustNotAllowReservation()
        {
            this.trainRepository.Setup(t => t.FindTrain(It.IsAny<int>())).Returns(() => new Train()
            {
                Capacity = 1,
                Coaches = new List<Coach>() {
                    new Coach ()
                    {
                    Lotation = 71,
                    Ocupation = 30
                    }
                }
            });
            var customerId = 1;
            var trainId = 1;
            var qty = 1;
            sut.ReserveSeat(customerId, trainId, qty);
        }

        [TestMethod]
        public void XPTO()
        {
            this.trainRepository.Setup(t => t.FindTrain(It.IsAny<int>())).Returns(() => new Train()
            {
                Capacity = 1,
                Coaches = new List<Coach>() {
                    new Coach ()
                    {
                    Lotation = 20

                    },
                    new Coach ()
                    {
                    Lotation = 60
                    },
                    new Coach ()
                    {
                    Lotation = 71
                    }
                }
            });
            var customerId = 1;
            var trainId = 1;
            var qty = 1;
            sut.ReserveSeat(customerId, trainId, qty);
        }
    }

    public interface ITrainRepository
    {
        Train FindTrain(int traindId);
    }

    public class ReservationService
    {
        private ITrainRepository trainRepository;

        public ReservationService(ITrainRepository trainRepository)
        {
            this.trainRepository = trainRepository;
        }

        public Train Train { get; set; }

        public void ReserveSeat(int customerId, int trainId, int qty)
        {
            VerifyTrainCapacity(trainId, qty);
        }

        private void VerifyTrainCapacity(int trainId, int qty)
        {
            var train = this.trainRepository.FindTrain(trainId);
            if (train.Capacity > 70 || train.Coaches.All(c => c.GetCapacity() > 70))
                throw new Exception();
        }
    }

    public class Train
    {
        public int Id { get; set; }

        public int Capacity { get; set; }

        public List<Coach> Coaches { get; set; }

        public Train()
        {
            this.Coaches = new List<Coach>();
        }
    }

    public class Coach
    {
        public int Lotation { get; set; }

        public int Ocupation { get; set; }

        public double GetCapacity()
        {
            if (Ocupation == 0)
            {
                return 0;
            }
            return ((Lotation / Ocupation) * 100);
        }
    }
}