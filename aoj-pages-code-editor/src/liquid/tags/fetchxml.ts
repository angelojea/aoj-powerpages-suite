import { Context, Emitter, Liquid, Tag, TagToken, TopLevelToken } from "liquidjs";
import { Parser } from "liquidjs/dist/parser";

export class FetchXmlTag extends Tag {
    private _xml: string = "";
    private _variableName: string = "";

    constructor(token: TagToken, remainTokens: TopLevelToken[], liquid: Liquid, parser: Parser) {
        super(token, remainTokens, liquid)
        const match = /\w+/.exec(token.args)
        if (!match || match.length! <= 0) throw new Error(`invalid variable name for tag ${token.getText()}`);

        while (remainTokens.length) {
            const token = remainTokens.shift()!
            //@ts-ignore
            if (token.name === `endfetchxml`) return;
            const template = parser.parseToken(token, remainTokens)
            this._variableName = match[0]!;
            //@ts-ignore
            this._xml = template.str;
        }
        throw new Error(`tag ${token.getText()} not closed`)
    }

    * render(ctx: Context, emitter: Emitter) {
        // TODO: fetch the data and then Drop
    }
}

