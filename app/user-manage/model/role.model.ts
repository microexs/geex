// import { Address } from './address.model';
import { prop, DocumentType, plugin, pre, Ref } from "@typegoose/typegoose";
import { PhoneNumberResolver, EmailAddressResolver } from "graphql-scalars";
import { ObjectId } from "mongodb";
import { Document, Model, Schema, Types } from "mongoose";
import { ModelBase } from "../../../shared/utils/model-base";
import { Injector } from "@graphql-modules/di";
import { ModelFieldResolver, IUserContext } from "../../../types";
import { ObjectType, Field } from "@nestjs/graphql";
import { NestContainer, ModulesContainer } from "@nestjs/core";
import { User } from "../../account/models/user.model";
import { UserRole } from "./user-role.model";


@ObjectType()

export class Role extends ModelBase {
    @prop({
        ref: Role,
        localField: nameof(Role.prototype._id),
        foreignField: nameof(UserRole.prototype.roleId),
    })
    public userRoles?: Ref<UserRole>[];

    @prop({
        unique: true,
    })
    @Field()
    public name!: string;

    /**
     *
     */
    constructor(name: string) {
        super();
        this.name = name;
    }
}
