import { GraphQLModule, GraphQLModuleOptions } from "@graphql-modules/core";
import { Injector, ProviderScope } from "@graphql-modules/di";
import { ApolloServer, IResolvers } from "apollo-server-express";
import { ExpressContext } from "apollo-server-express/dist/ApolloServer";
import express = require("express");
import { Connection, createConnection } from "mongoose";
import { environment } from "../environments/environment";
import { LoggingMiddleware } from "./audit-log/audit-log.middleware";
import { AuditLogModule } from "./audit-log/audit-log.module";
import { AuthModule } from "./auth/auth.module";
import { AuthConfigToken, GeexServerConfigToken, LoggerConfigToken, TracingConfigToken, UserModelToken } from "./tokens";
import { IGeexContext, IGeexServerConfig } from "../types";
import { GeexLogger } from "./utils/logger";
import OpentracingExtension from "apollo-opentracing";
import { Tracer } from "opentracing";
import { JaegerTraceExtension } from "./extensions/jaeger-trace.gql-extension";
import { ComplexityExtension } from "./extensions/complexity.gql-extension";
import { getModelForClass } from "@typegoose/typegoose";

const result = new GraphQLModule<IGeexServerConfig, ExpressContext, IGeexContext>({
    providers: ({ config }) => [
        {
            provide: GeexServerConfigToken,
            useValue: config,
        }, {
            provide: LoggerConfigToken,
            useValue: config.loggerConfig,
        }, {
            provide: AuthConfigToken,
            useValue: config.authConfig,
        }, {
            provide: TracingConfigToken,
            useValue: config.traceConfig,
        },
        GeexLogger,
        LoggingMiddleware,
        JaegerTraceExtension,
        ComplexityExtension,
    ],
    imports: [AuditLogModule, AuthModule],
}, environment);

export const SharedModule = result;
