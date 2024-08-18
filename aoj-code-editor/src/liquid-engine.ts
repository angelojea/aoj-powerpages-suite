import { Context, Drop, Emitter, Liquid, Tag, TagToken, Template, TopLevelToken } from "liquidjs";
import { Parser } from "liquidjs/dist/parser";

class ProductsDrop extends Drop {
    products = [
        { title: Date.now() },
        { title: Date.now() },
        { title: Date.now() },
        { title: Date.now() },
        { title: Date.now() },
        { title: Date.now() },
        { title: Date.now() },
    ];
}

export const liquidEngine = {
    render: (code: string) => {
        var engine = new Liquid();

        engine.registerTag('fetchxml', class FetchxmlParser extends Tag {
            templates: { varName: string, template: Template }[] = []
            constructor(token: TagToken, remainTokens: TopLevelToken[], liquid: Liquid, parser: Parser) {
                super(token, remainTokens, liquid)
                const match = /\w+/.exec(token.args)
                if (!match || match.length! <= 0) throw new Error(`invalid variable name for tag ${token.getText()}`);

                while (remainTokens.length) {
                    const token = remainTokens.shift()!
                    //@ts-ignore
                    if (token.name === `endfetchxml`) return
                    const template = parser.parseToken(token, remainTokens)
                    this.templates.push({ varName: match[0]!, template: template })
                }
                throw new Error(`tag ${token.getText()} not closed`)
            }

            * render(ctx: Context, emitter: Emitter) {
                //@ts-ignore
                const xml = this.templates[0].template.str;
                //@ts-ignore
                // const data = yield fetch();
                emitter.write(encodeURIComponent(xml))
            }
        });

        return engine.parseAndRenderSync(code);
    }
}
