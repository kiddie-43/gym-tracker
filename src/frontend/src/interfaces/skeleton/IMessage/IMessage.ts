import { Message } from "../../../enums/message/message";

export interface IMessage {
    code: Message;
    message: string;
    title?: string;

}