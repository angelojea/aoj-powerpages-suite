import { Context, Drop, Emitter, Liquid, Tag, TagToken, Template, TopLevelToken } from "liquidjs";
import { Parser } from "liquidjs/dist/parser";
import { FetchXmlTag } from "./tags/fetchxml";
import { IncludeTag } from "./tags/include";

export const liquidEngine = {
    render: (code: string) => {
        var engine = new Liquid();

        engine.registerTag('fetchxml', FetchXmlTag);
        engine.registerTag('include', IncludeTag);

        return engine.parseAndRenderSync(code);
    }
}
