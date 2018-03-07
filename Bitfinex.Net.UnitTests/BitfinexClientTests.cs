using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bitfinex.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.RateLimiter;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Bitfinex.Net.UnitTests
{
    [TestFixture()]
    public class BitfinexClientTests
    {
        [TestCase]
        public void GetPlatformStatus_Should_RespondWithPlatformStatus()
        {
            // arrange
            var expected = new BitfinexPlatformStatus()
            {
                Status = PlatformStatus.Operative
            };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[] { (int)expected.Status }));

            // act
            var result = client.GetPlatformStatus();

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetTicker_Should_RespondWithPrices()
        {
            // arrange
            var expected = new []
            {
                new BitfinexMarketOverview()
                {
                    Ask = 0.1m,
                    AskSize = 0.2m,
                    Bid = 0.3m,
                    BidSize = 0.4m,
                    DailtyChangePercentage = 0.5m,
                    DailyChange = 0.6m,
                    High = 0.7m,
                    LastPrice = 0.8m,
                    Low = 0.9m,
                    Volume = 1.1m,
                    Symbol = "Test"
                }
            };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[] { new object[] { "Test", 0.3m, 0.4m, 0.1m, 0.2m, 0.6m, 0.5m, 0.8m, 1.1m, 0.7m, 0.9m } }));

            // act
            var result = client.GetTicker("Test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
        }

        [TestCase]
        public void GetTrades_Should_RespondWithPrices()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexTradeSimple()
                {
                    Amount = 0.1m,
                    Id = 1,
                    Price = 0.2m,
                    Timestamp = new DateTime(2017, 1, 1)
                },
                new BitfinexTradeSimple()
                {
                    Amount = 0.3m,
                    Id = 2,
                    Price = 0.4m,
                    Timestamp = new DateTime(2016, 1, 1)
                }
            };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, GetTimestamp(new DateTime(2017, 1, 1)), 0.1m, 0.2m},
                new object[] { 2, GetTimestamp(new DateTime(2016, 1, 1)), 0.3m, 0.4m}
            }));

            // act
            var result = client.GetTrades("Test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetOrderbook_Should_RespondWithOrderbook()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexOrderBookEntry()
                {
                    Amount = 0.1m,
                    Price = 0.2m,
                    Count = 1
                },
                new BitfinexOrderBookEntry()
                {
                    Amount = 0.3m,
                    Price = 0.4m,
                    Count = 2
                }
            };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 0.2m, 1, 0.1m},
                new object[] { 0.4m, 2, 0.3m}
            }));

            // act
            var result = client.GetOrderBook("Test", Precision.PrecisionLevel0);

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetStats_Should_RespondWithStats()
        {
            // arrange
            var expected =
                new BitfinexStats()
                {
                    Timestamp = new DateTime(2017, 1 ,1),
                    Value = 0.1m
                };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[]{ GetTimestamp(new DateTime(2017,1,1)), 0.1m }));

            // act
            var result = client.GetStats("test", StatKey.ActiveFundingInPositions, StatSide.Long, StatSection.History);

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetLastCandle_Should_RespondWithCandle()
        {
            // arrange
            var expected =
                new BitfinexCandle()
                {
                    Timestamp = new DateTime(2017, 1, 1),
                    Volume = 0.1m,
                    Low = 0.2m,
                    High = 0.3m,
                    Close = 0.4m,
                    Open = 0.5m
                };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[] { GetTimestamp(new DateTime(2017, 1, 1)), 0.5m, 0.4m, 0.3m, 0.2m, 0.1m }));

            // act
            var result = client.GetLastCandle(TimeFrame.FiveMinute, "test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetCandles_Should_RespondWithCandles()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexCandle()
                {
                    Timestamp = new DateTime(2017, 1, 1),
                    Volume = 0.1m,
                    Low = 0.2m,
                    High = 0.3m,
                    Close = 0.4m,
                    Open = 0.5m
                },
                new BitfinexCandle()
                {
                    Timestamp = new DateTime(2016, 1, 1),
                    Volume = 0.6m,
                    Low = 0.7m,
                    High = 0.8m,
                    Close = 0.9m,
                    Open = 1.1m
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { GetTimestamp(new DateTime(2017, 1, 1)), 0.5m, 0.4m, 0.3m, 0.2m, 0.1m },
                new object[] { GetTimestamp(new DateTime(2016, 1, 1)), 1.1m, 0.9m, 0.8m, 0.7m, 0.6m }
            }));

            // act
            var result = client.GetCandles(TimeFrame.FiveMinute, "test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetMarketAveragePrice_Should_RespondWithAveragePrice()
        {
            // arrange
            var expected =
                new BitfinexMarketAveragePrice()
                {
                    Amount = 0.1m,
                    AverageRate = 0.2m
                };

            var client = PrepareClient(JsonConvert.SerializeObject(new object[] { 0.2m, 0.1m }));

            // act
            var result = client.GetMarketAveragePrice("test", 0.1m, 0.2m);

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetWallets_Should_RespondWithWallets()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexWallet()
                {
                    Balance = 0.1m,
                    BalanceAvailable = 0.2m,
                    Currency = "test",
                    Type = WalletType.Exchange,
                    UnsettledInterest = 0.3m
                },
                new BitfinexWallet()
                {
                    Balance = 0.4m,
                    BalanceAvailable = 0.5m,
                    Currency = "test2",
                    Type = WalletType.Funding,
                    UnsettledInterest = 0.6m
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { "exchange", "test", 0.1m, 0.3m, 0.2m },
                new object[] { "funding", "test2", 0.4m, 0.6m, 0.5m },
            }));

            // act
            var result = client.GetWallets();

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetActiveOrders_Should_RespondWithActiveOrders()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexOrder()
                {
                    Amount = 0.1m,
                    Price = 0.2m,
                    Id = 1,
                    Status = OrderStatus.Active,
                    Type = OrderType.ExchangeFillOrKill,
                    Symbol = "test",
                    AmountOriginal = 0.3m,
                    ClientOrderId = 2,
                    Flags = 0,
                    GroupId = null,
                    Hidden = false,
                    Notify = false,
                    PlacedId = 3,
                    PriceAuxilliaryLimit = 0,
                    PriceAverage = 0.4m,
                    PriceTrailing = 0.5m,
                    TimestampCreated = new DateTime(2017,1,1),
                    TimestampUpdated = new DateTime(2017,1,1),
                    TypePrevious = null
                },
                new BitfinexOrder()
                {
                    Amount = 0.6m,
                    Price = 0.7m,
                    Id = 4,
                    Status = OrderStatus.Active,
                    Type = OrderType.Limit,
                    Symbol = "test",
                    AmountOriginal = 0.8m,
                    ClientOrderId = 5,
                    Flags = 0,
                    GroupId = null,
                    Hidden = false,
                    Notify = false,
                    PlacedId = 6,
                    PriceAuxilliaryLimit = 0,
                    PriceAverage = 0.9m,
                    PriceTrailing = 1.1m,
                    TimestampCreated = new DateTime(2016,1,1),
                    TimestampUpdated = new DateTime(2016,1,1),
                    TypePrevious = null
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, null, 2, "test", GetTimestamp(new DateTime(2017,1,1)), GetTimestamp(new DateTime(2017,1,1)), 0.1m, 0.3m, "EXCHANGE FOK", null, null, null, 0, "ACTIVE", null, null, 0.2m, 0.4m, 0.5m, 0, null, null, null, 0, 0, 3 },
                new object[] { 4, null, 5, "test", GetTimestamp(new DateTime(2016,1,1)), GetTimestamp(new DateTime(2016,1,1)), 0.6m, 0.8m, "LIMIT", null, null, null, 0, "ACTIVE", null, null, 0.7m, 0.9m, 1.1m, 0, null, null, null, 0, 0, 6 },
            }));

            // act
            var result = client.GetActiveOrders();

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetTradesForOrder_Should_RespondWithTrades()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexTradeDetails()
                {
                    Id = 1,
                    TimestampCreated = new DateTime(2017,1,1),
                    ExecutedAmount = 0.1m,
                    ExecutedPrice = 0.2m,
                    Fee = 0.3m,
                    FeeCurrency = "Test",
                    Maker = false,
                    OrderId = 2,
                    OrderPrice = 0.4m,
                    OrderType = OrderType.Limit,
                    Pair = "TEST"
                },
                new BitfinexTradeDetails()
                {
                    Id = 3,
                    TimestampCreated = new DateTime(2016,1,1),
                    ExecutedAmount = 0.5m,
                    ExecutedPrice = 0.6m,
                    Fee = 0.7m,
                    FeeCurrency = "Test",
                    Maker = false,
                    OrderId = 4,
                    OrderPrice = 0.8m,
                    OrderType = OrderType.Market,
                    Pair = "TEST"
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, "TEST", GetTimestamp(new DateTime(2017,1,1)), 2, 0.1m, 0.2m, "LIMIT", 0.4m, -1, 0.3m, "Test" },
                new object[] { 3, "TEST", GetTimestamp(new DateTime(2016,1,1)), 4, 0.5m, 0.6m, "MARKET", 0.8m, -1, 0.7m, "Test" },
            }));

            // act
            var result = client.GetTradesForOrder("TEST", 1);

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetActivePositions_Should_RespondWithPositions()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexPosition()
                {
                    Amount = 0.1m,
                    Symbol = "Test",
                    Status = PositionStatus.Active,
                    BasePrice = 0.2m,
                    Leverage = 0.3m,
                    LiquidationPrice = 0.4m,
                    MarginFunding = 0.5m,
                    MarginFundingType = MarginFundingType.Daily,
                    ProfitLoss = 0.6m,
                    ProfitLossPercentage = 0.7m
                },
                new BitfinexPosition()
                {
                    Amount = 0.8m,
                    Symbol = "Test2",
                    Status = PositionStatus.Closed,
                    BasePrice = 0.9m,
                    Leverage = 1.1m,
                    LiquidationPrice = 1.2m,
                    MarginFunding = 1.3m,
                    MarginFundingType = MarginFundingType.Term,
                    ProfitLoss = 1.4m,
                    ProfitLossPercentage = 1.5m
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { "Test", "ACTIVE", 0.1m, 0.2m, 0.5m, 0, 0.6m, 0.7m, 0.4m, 0.3m },
                new object[] { "Test2", "CLOSED", 0.8m, 0.9m, 1.3m, 1, 1.4m, 1.5m, 1.2m, 1.1m },
            }));

            // act
            var result = client.GetActivePositions();

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetActiveFundingOffers_Should_RespondWithFundingOffers()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexFundingOffer()
                {
                    Amount = 0.1m,
                    Symbol = "Test",
                    Id = 1,
                    Status = OrderStatus.Active,
                    TimestampCreated = new DateTime(2017,1,1),
                    TimestampUpdated = new DateTime(2017,1,1),
                    Flags = 0,
                    AmountOriginal = 0.2m,
                    Notify = false,
                    Hidden = false,
                    FundingType = FundingType.Lend,
                    Period = 1,
                    Rate = 0.3m,
                    RateReal = 0.4m,
                    Renew = false
                },
                new BitfinexFundingOffer()
                {
                    Amount = 0.5m,
                    Symbol = "Test",
                    Id = 2,
                    Status = OrderStatus.Canceled,
                    TimestampCreated = new DateTime(2016,1,1),
                    TimestampUpdated = new DateTime(2016,1,1),
                    Flags = 0,
                    AmountOriginal = 0.6m,
                    Notify = true,
                    Hidden = true,
                    FundingType = FundingType.Loan,
                    Period = 2,
                    Rate = 0.7m,
                    RateReal = 0.8m,
                    Renew = true
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, "Test", GetTimestamp(new DateTime(2017,1,1)), GetTimestamp(new DateTime(2017,1,1)), 0.1m, 0.2m, "lend", null,null,0, "ACTIVE", null, null,null, 0.3m, 1, 0, 0, null, 0, 0.4m },
                new object[] { 2, "Test", GetTimestamp(new DateTime(2016,1,1)), GetTimestamp(new DateTime(2016,1,1)), 0.5m, 0.6m, "loan", null,null,0, "CANCELED", null, null,null, 0.7m, 2, 1, 1, null, 1, 0.8m },
            }));

            // act
            var result = client.GetActiveFundingOffers("Test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetFundingLoans_Should_RespondWithFundingLoans()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexFundingLoan()
                {
                    Amount = 0.1m,
                    Symbol = "Test",
                    Id = 1,
                    Status = OrderStatus.Active,
                    TimestampCreated = new DateTime(2017,1,1),
                    TimestampUpdated = new DateTime(2017,1,1),
                    Flags = 0,
                    Notify = false,
                    Hidden = false,
                    Period = 1,
                    Rate = 0.3m,
                    RateReal = 0.4m,
                    Renew = false,
                    NoClose = false,
                    Side = FundingType.Lend,
                    TimestampLastPayout = new DateTime(2017,1,1),
                    TimestampOpened = new DateTime(2017,1,1)
                },
                new BitfinexFundingLoan()
                {
                    Amount = 0.5m,
                    Symbol = "Test",
                    Id = 2,
                    Status = OrderStatus.Canceled,
                    TimestampCreated = new DateTime(2016,1,1),
                    TimestampUpdated = new DateTime(2016,1,1),
                    Flags = 0,
                    Notify = true,
                    Hidden = true,
                    Period = 2,
                    Rate = 0.7m,
                    RateReal = 0.8m,
                    Renew = true,
                    NoClose = true,
                    Side = FundingType.Loan,
                    TimestampLastPayout = new DateTime(2016,1,1),
                    TimestampOpened = new DateTime(2016,1,1)
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, "Test", "lend", GetTimestamp(new DateTime(2017,1,1)), GetTimestamp(new DateTime(2017,1,1)), 0.1m, 0, "ACTIVE", null, null,null, 0.3m, 1, GetTimestamp(new DateTime(2017, 1, 1)), GetTimestamp(new DateTime(2017, 1, 1)), 0, 0, null, 0, 0.4m, 0},
                new object[] { 2, "Test", "loan", GetTimestamp(new DateTime(2016,1,1)), GetTimestamp(new DateTime(2016,1,1)), 0.5m, 0, "CANCELED", null, null,null, 0.7m, 2, GetTimestamp(new DateTime(2016, 1, 1)), GetTimestamp(new DateTime(2016, 1, 1)), 1, 1, null, 1, 0.8m, 1 },
            }));

            // act
            var result = client.GetFundingLoans("Test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetFundingCredits_Should_RespondWithFundingCredits()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexFundingCredit()
                {
                    Amount = 0.1m,
                    Symbol = "Test",
                    Id = 1,
                    Status = OrderStatus.Active,
                    TimestampCreated = new DateTime(2017,1,1),
                    TimestampUpdated = new DateTime(2017,1,1),
                    Flags = 0,
                    Notify = false,
                    Hidden = false,
                    Period = 1,
                    Rate = 0.3m,
                    RateReal = 0.4m,
                    Renew = false,
                    NoClose = false,
                    Side = FundingType.Lend,
                    TimestampLastPayout = new DateTime(2017,1,1),
                    TimestampOpened = new DateTime(2017,1,1),
                    PositionPair = "Test"
                },
                new BitfinexFundingCredit()
                {
                    Amount = 0.5m,
                    Symbol = "Test",
                    Id = 2,
                    Status = OrderStatus.Canceled,
                    TimestampCreated = new DateTime(2016,1,1),
                    TimestampUpdated = new DateTime(2016,1,1),
                    Flags = 0,
                    Notify = true,
                    Hidden = true,
                    Period = 2,
                    Rate = 0.7m,
                    RateReal = 0.8m,
                    Renew = true,
                    NoClose = true,
                    Side = FundingType.Loan,
                    TimestampLastPayout = new DateTime(2016,1,1),
                    TimestampOpened = new DateTime(2016,1,1),
                    PositionPair = "Test"
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, "Test", "lend", GetTimestamp(new DateTime(2017,1,1)), GetTimestamp(new DateTime(2017,1,1)), 0.1m, 0, "ACTIVE", null, null,null, 0.3m, 1, GetTimestamp(new DateTime(2017, 1, 1)), GetTimestamp(new DateTime(2017, 1, 1)), 0, 0, null, 0, 0.4m, 0, "Test"},
                new object[] { 2, "Test", "loan", GetTimestamp(new DateTime(2016,1,1)), GetTimestamp(new DateTime(2016,1,1)), 0.5m, 0, "CANCELED", null, null,null, 0.7m, 2, GetTimestamp(new DateTime(2016, 1, 1)), GetTimestamp(new DateTime(2016, 1, 1)), 1, 1, null, 1, 0.8m, 1, "Test"},
            }));

            // act
            var result = client.GetFundingCredits("Test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetFundingTradesHistory_Should_RespondWithFundingTrades()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexFundingTrade()
                {
                    Id = 1,
                    Amount = 0.1m,
                    Timestamp = new DateTime(2017,1,1),
                    Rate = 0.2m,
                    Period = 2,
                    Currency = "Test",
                    Maker = false,
                    OfferId = 3
                },
                new BitfinexFundingTrade()
                {
                    Id = 4,
                    Amount = 0.3m,
                    Timestamp = new DateTime(2016,1,1),
                    Rate = 0.4m,
                    Period = 5,
                    Currency = "Test",
                    Maker = true,
                    OfferId = 6
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] { 1, "Test", GetTimestamp(new DateTime(2017,1,1)), 3, 0.1m, 0.2m, 2, 0},
                new object[] { 4, "Test", GetTimestamp(new DateTime(2016,1,1)), 6, 0.3m, 0.4m, 5, 1},
            }));

            // act
            var result = client.GetFundingTradesHistory("Test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[1], result.Data[1]));
        }

        [TestCase]
        public void GetBaseMargin_Should_RespondWithMargin()
        {
            // arrange
            var expected = new BitfinexMarginBase()
            {
                Type = "base",
                Data = new BitfinexMarginBaseInfo()
                {
                    MarginBalance = 0.1m,
                    MarginNet = 0.2m,
                    UserProfitLoss = 0.3m,
                    UserSwapsAmount = 0.4m
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                "base", new object[] { 0.3m, 0.4m, 0.1m, 0.2m}
            }));

            // act
            var result = client.GetBaseMarginInfo();

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetSymbolMargin_Should_RespondWithMargin()
        {
            // arrange
            var expected = new BitfinexMarginSymbol()
            {
                Symbol = "test",
                Data = new BitfinexMarginSymbolInfo()
                {
                    Buy = 0.1m,
                    GrossBalance = 0.2m,
                    Sell = 0.3m,
                    TradeableBalance = 0.4m
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                "test", new object[] { 0.4m, 0.2m, 0.1m, 0.3m}
            }));

            // act
            var result = client.GetSymbolMarginInfo("test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetFundingInfo_Should_RespondWithFundingInfo()
        {
            // arrange
            var expected = new BitfinexFundingInfo()
            {
                Symbol = "test",
                Type = "sym",
                Data = new BitfinexFundingInfoDetails()
                {
                    DurationLend = 0.1m,
                    DurationLoan = 0.2m,
                    YieldLend = 0.3m,
                    YieldLoan = 0.4m
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                "sym", "test", new object[] { 0.4m, 0.3m, 0.2m, 0.1m}
            }));

            // act
            var result = client.GetFundingInfo("test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected, result.Data));
        }

        [TestCase]
        public void GetMovements_Should_RespondWithMovements()
        {
            // arrange
            var expected = new[]
            {
                new BitfinexMovement()
                {
                    Id = "test",
                    Amount = 0.1m,
                    Status = "Status",
                    Currency = "Cur",
                    Address = "add",
                    CurrencyName = "curname",
                    Fees = 0.2m,
                    Started = new DateTime(2017, 1, 1),
                    TransactionId = "tx",
                    Updated = new DateTime(2017, 1, 1)
                }
            };
            var client = PrepareClient(JsonConvert.SerializeObject(new object[]
            {
                new object[] {"test", "Cur", "curname", null,null, GetTimestamp(new DateTime(2017,1,1)), GetTimestamp(new DateTime(2017,1,1)), null, null, "Status", null, null, 0.1m, 0.2m, null, null, "add", null, null, null, "tx", null }
            }));

            // act
            var result = client.GetMovements("test");

            // assert
            Assert.AreEqual(true, result.Success);
            Assert.IsTrue(ObjectComparer.PublicInstancePropertiesEqual(expected[0], result.Data[0]));
        }

        private BitfinexClient PrepareClient(string responseData, bool credentials = true)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(responseData);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var response = new Mock<IResponse>();
            response.Setup(c => c.GetResponseStream()).Returns(responseStream);

            var request = new Mock<IRequest>();
            request.Setup(c => c.Headers).Returns(new WebHeaderCollection());
            request.Setup(c => c.GetResponse()).Returns(Task.FromResult(response.Object));
            request.Setup(c => c.GetRequestStream()).Returns(Task.FromResult((Stream)new MemoryStream()));

            var factory = new Mock<IRequestFactory>();
            factory.Setup(c => c.Create(It.IsAny<string>()))
                .Returns(request.Object);

            BitfinexClient client = credentials ? new BitfinexClient(new BitfinexClientOptions() { ApiCredentials = new ApiCredentials("test", "test") }) : new BitfinexClient();
            client.RequestFactory = factory.Object;
            return client;
        }

        private long GetTimestamp(DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}