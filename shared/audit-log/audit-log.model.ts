import { prop } from "@typegoose/typegoose";
import { Field, ObjectType } from "type-graphql";

@ObjectType()
export class AuditLog {
    @Field()
    public content: string;
    /**
     *
     */
    constructor(content: string) {
        this.content = content;
    }
}
