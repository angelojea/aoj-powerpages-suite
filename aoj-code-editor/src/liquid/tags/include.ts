import { Context, Emitter, Liquid, Tag, TagToken, TopLevelToken } from "liquidjs";
import { Parser } from "liquidjs/dist/parser";

export class IncludeTag extends Tag {
    private _templateName: string = "";

    constructor(token: TagToken, remainTokens: TopLevelToken[], liquid: Liquid, parser: Parser) {
        super(token, remainTokens, liquid)
        const templateName = token.args.replace(/(^\')|(^\")|(\'$)|(\"$)/g, '');
        
        this._templateName = templateName;
    }

    * render(ctx: Context, emitter: Emitter) {
        emitter.write(`<span>Web Template with name ${this._templateName} goes here</span>`);
    }
}

