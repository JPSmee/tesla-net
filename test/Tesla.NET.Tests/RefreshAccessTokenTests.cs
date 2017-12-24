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
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using Tesla.NET.Models;
    using Xunit;

    public abstract class RefreshAccessTokenSuccessTestsBase : AuthRequestContext
    {
        private readonly AccessTokenResponse _expected;
        private readonly Uri _expectedRefreshUri;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _refreshToken;

        protected RefreshAccessTokenSuccessTestsBase(bool useCustomBaseUri)
            : base(useCustomBaseUri)
        {
            // Arrange
            _expected = Fixture.Create<AccessTokenResponse>();
            Handler.SetResponseContent(_expected);
            _expectedRefreshUri = new Uri(BaseUri, "oauth/token");

            _clientId = Fixture.Create("clientId");
            _clientSecret = Fixture.Create("clientSecret");
            _refreshToken = Fixture.Create("refreshToken");
        }

        [Fact]
        public async Task Should_make_a_POST_request()
        {
            // Act_refreshToken
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            Handler.Request.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public async Task Should_refresh_the_OAuth_Token_endpoint()
        {
            // Act
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            Handler.Request.RequestUri.Should().Be(_expectedRefreshUri);
        }

        [Fact]
        public async Task Should_return_the_expected_access_token()
        {
            // Act
            MessageResponse<AccessTokenResponse> actual =
                await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                    .ConfigureAwait(false);

            // Assert
            actual.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            actual.Data.AsLikeness().ShouldEqual(_expected);
        }

        [Fact]
        public async Task Should_make_a_POST_4_parameters()
        {
            // Act
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            string requestContent = Handler.RequestContents[0];
            Dictionary<string, StringValues> formParameters = QueryHelpers.ParseQuery(requestContent);

            formParameters.Should().HaveCount(4);
        }

        [Fact]
        public async Task Should_make_a_POST_the_grant_type_parameter()
        {
            // Act
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            string requestContent = Handler.RequestContents[0];
            Dictionary<string, StringValues> formParameters = QueryHelpers.ParseQuery(requestContent);

            formParameters
                .Should().ContainKey("grant_type")
                .WhichValue.Should().HaveCount(1)
                .And.ContainSingle("refresh_token");
        }

        [Fact]
        public async Task Should_make_a_POST_the_client_id_parameter()
        {
            // Act
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            string requestContent = Handler.RequestContents[0];
            Dictionary<string, StringValues> formParameters = QueryHelpers.ParseQuery(requestContent);

            formParameters
                .Should().ContainKey("client_id")
                .WhichValue.Should().HaveCount(1)
                .And.ContainSingle(_clientId);
        }

        [Fact]
        public async Task Should_make_a_POST_the_client_secret_parameter()
        {
            // Act
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            string requestContent = Handler.RequestContents[0];
            Dictionary<string, StringValues> formParameters = QueryHelpers.ParseQuery(requestContent);

            formParameters
                .Should().ContainKey("client_secret")
                .WhichValue.Should().HaveCount(1)
                .And.ContainSingle(_clientSecret);
        }

        [Fact]
        public async Task Should_make_a_POST_the_refresh_token_parameter()
        {
            // Act
            await Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken)
                .ConfigureAwait(false);

            // Assert
            string requestContent = Handler.RequestContents[0];
            Dictionary<string, StringValues> formParameters = QueryHelpers.ParseQuery(requestContent);

            formParameters
                .Should().ContainKey("refresh_token")
                .WhichValue.Should().HaveCount(1)
                .And.ContainSingle(_refreshToken);
        }
    }

    public class When_refreshing_an_access_token_using_the_default_base_Uri : RefreshAccessTokenSuccessTestsBase
    {
        public When_refreshing_an_access_token_using_the_default_base_Uri()
            : base(useCustomBaseUri: false)
        {
        }
    }

    public class When_refreshing_an_access_token_using_a_custom_base_Uri : RefreshAccessTokenSuccessTestsBase
    {
        public When_refreshing_an_access_token_using_a_custom_base_Uri()
            : base(useCustomBaseUri: true)
        {
        }
    }

    public abstract class RefreshAccessTokenFailureTestsBase : AuthRequestContext
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _refreshToken;

        protected RefreshAccessTokenFailureTestsBase(bool useCustomBaseUri)
            : base(useCustomBaseUri)
        {
            // Arrange
            Handler.SetResponseContent(new JObject(), HttpStatusCode.Unauthorized);

            _clientId = Fixture.Create("clientId");
            _clientSecret = Fixture.Create("clientSecret");
            _refreshToken = Fixture.Create("refreshToken");
        }

        [Fact]
        public void Should_throw_an_HttpRefreshException()
        {
            // Act
            Func<Task> action = () => Sut.RefreshAccessTokenAsync(_clientId, _clientSecret, _refreshToken);

            // Assert
            action.ShouldThrowExactly<HttpRequestException>();
        }
    }

    public class When_failing_to_refresh_an_access_token_using_the_default_base_Uri
        : RefreshAccessTokenFailureTestsBase
    {
        public When_failing_to_refresh_an_access_token_using_the_default_base_Uri()
            : base(useCustomBaseUri: false)
        {
        }
    }

    public class When_failing_to_refresh_an_access_token_using_a_custom_base_Uri
        : RefreshAccessTokenFailureTestsBase
    {
        public When_failing_to_refresh_an_access_token_using_a_custom_base_Uri()
            : base(useCustomBaseUri: true)
        {
        }
    }
}
