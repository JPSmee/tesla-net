﻿// Copyright (c) 2018 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Tesla.NET
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using Tesla.NET.Models;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class GetVehiclesSuccessTestsBase : ClientRequestContext
    {
        private readonly ResponseDataWrapper<IReadOnlyList<Vehicle>> _expected;
        private readonly Uri _expectedRequestUri;

        protected GetVehiclesSuccessTestsBase(ITestOutputHelper output, bool useCustomBaseUri)
            : base(output, useCustomBaseUri)
        {
            // Arrange
            _expected = Fixture.Create<ResponseDataWrapper<IReadOnlyList<Vehicle>>>();
            Handler.SetResponseContent(_expected);
            _expectedRequestUri = new Uri(BaseUri, "api/1/vehicles");
        }

        [Fact]
        public async Task Should_make_a_GET_request()
        {
            // Act
            await Sut.GetVehiclesAsync(AccessToken).ConfigureAwait(false);

            // Assert
            Handler.Request.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async Task Should_request_the_vehicles_endpoint()
        {
            // Act
            await Sut.GetVehiclesAsync(AccessToken).ConfigureAwait(false);

            // Assert
            Handler.Request.RequestUri.Should().Be(_expectedRequestUri);
        }

        [Fact]
        public async Task Should_return_the_expected_vehicles()
        {
            // Act
            MessageResponse<ResponseDataWrapper<IReadOnlyList<Vehicle>>> actual =
                await Sut.GetVehiclesAsync(AccessToken).ConfigureAwait(false);

            // Assert
            actual.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            actual.Data.ShouldBeEquivalentTo(_expected, WithStrictOrdering);
        }

        [Fact]
        public async Task Should_set_the_bearer_token_with_the_specified_access_token()
        {
            // set_the_bearer_token_when_specified
            await Sut.GetVehiclesAsync(AccessToken).ConfigureAwait(false);

            // Assert
            Handler.Request.Headers.Authorization.Scheme.Should().Be("Bearer");
            Handler.Request.Headers.Authorization.Parameter.Should().Be(AccessToken);
        }

        [Fact]
        public async Task Should_NOT_set_the_bearer_token_if_the_access_token_is_not_specified()
        {
            // set_the_bearer_token_when_specified
            await Sut.GetVehiclesAsync().ConfigureAwait(false);

            // Assert
            Handler.Request.Headers.Authorization.Should().BeNull();
        }
    }

    public class When_getting_the_vehicles_for_an_account_using_the_default_base_Uri : GetVehiclesSuccessTestsBase
    {
        public When_getting_the_vehicles_for_an_account_using_the_default_base_Uri(ITestOutputHelper output)
            : base(output, useCustomBaseUri: false)
        {
        }
    }

    public class When_getting_the_vehicles_for_an_account_using_a_custom_base_Uri : GetVehiclesSuccessTestsBase
    {
        public When_getting_the_vehicles_for_an_account_using_a_custom_base_Uri(ITestOutputHelper output)
            : base(output, useCustomBaseUri: true)
        {
        }
    }

    public abstract class GetVehiclesFailureTestsBase : ClientRequestContext
    {
        protected GetVehiclesFailureTestsBase(ITestOutputHelper output, bool useCustomBaseUri)
            : base(output, useCustomBaseUri)
        {
            // Arrange
            Handler.SetResponseContent(new JObject(), HttpStatusCode.BadGateway);
        }

        [Fact]
        public void Should_throw_an_HttpRequestException()
        {
            // Act
            Func<Task> action = () => Sut.GetVehiclesAsync(AccessToken);

            // Assert
            action.ShouldThrowExactly<HttpRequestException>();
        }
    }

    public class When_failing_to_get_the_vehicles_for_an_account_using_the_default_base_Uri
        : GetVehiclesFailureTestsBase
    {
        public When_failing_to_get_the_vehicles_for_an_account_using_the_default_base_Uri(ITestOutputHelper output)
            : base(output, useCustomBaseUri: false)
        {
        }
    }

    public class When_failing_to_get_the_vehicles_for_an_account_using_a_custom_base_Uri
        : GetVehiclesFailureTestsBase
    {
        public When_failing_to_get_the_vehicles_for_an_account_using_a_custom_base_Uri(ITestOutputHelper output)
            : base(output, useCustomBaseUri: true)
        {
        }
    }
}
