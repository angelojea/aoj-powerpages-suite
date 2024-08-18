import React from 'react';
import ReactDOM from 'react-dom';
import * as Babel from '@babel/standalone';
import { liquidEngine } from './liquid/liquid-engine';
import { Console } from 'console';
import { PanelGroup, Panel, PanelResizeHandle } from 'react-resizable-panels';
import { DefaultButton, Stack } from '@fluentui/react';
import { NeutralColors } from '@fluentui/theme';
import Box from './ui/Box';
import { getStringBetweenTags } from './utils/string';
import Editor from './ui/Editor';

type EditorType = "html" | "css" | "js";

function App() {
  const iframeRef = React.useRef<HTMLIFrameElement>(null);
  const [isValid, setIsValid] = React.useState(true);
  const [selectedEditor, setSelectedEditor] = React.useState<EditorType>("html");
  const [htmlCode, setHtmlCode] = React.useState(``);
  const [cssCode, setCssCode] = React.useState(``);
  const [jsCode, setJsCode] = React.useState(``);

  const selectionBtns: { label: string, value: EditorType }[] = [
    { label: 'HTML', value: 'html' },
    { label: 'CSS', value: 'css' },
    { label: 'Javascript', value: 'js' },
  ]

  const renderCode = (htmlCode: string, cssCode: string, jsCode: string) => {

    setHtmlCode(htmlCode);
    setCssCode(cssCode);
    setJsCode(jsCode);

    setIsValid(true);
    try {
      if (!iframeRef.current) throw Error("");

      const hydratedCode = `${Boolean(cssCode) ? `<style>
${cssCode}
</style>` : ''}
${Boolean(htmlCode) ? `
${htmlCode}` : ''}
${Boolean(jsCode) ? `<script>
${jsCode}
</script>` : ''}`;

      const liquidConverted = liquidEngine.render(hydratedCode);
      const scriptContent = getStringBetweenTags(liquidConverted, `<script>`, `</script>`);

      const transpiled = Babel.transform(scriptContent, { presets: ['react'], }).code || "";


      const iframeWindow = iframeRef.current?.contentWindow! as Window;
      const iframeDocument = iframeRef.current?.contentDocument! as Document;
      iframeDocument.head.insertAdjacentHTML(`beforeend`, `<style>
        body {
          font-family: 'Segoe UI', 'Roboto', 'Oxygen',
          'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
          sans-serif;
          word-break: break-all;
        }
      </style>`);
      //@ts-ignore
      iframeWindow.React = React;
      //@ts-ignore
      iframeWindow.ReactDOM = ReactDOM;

      iframeDocument.body.innerHTML = '';
      iframeDocument.body.insertAdjacentHTML('afterbegin', liquidConverted);

      //@ts-ignore
      iframeWindow.aojError = (err) => {
        // TODO: Decide what to do with compilation errors
        // console.error(err);
        setIsValid(false);
      };
      //@ts-ignore
      iframeWindow.eval(`
        try {${transpiled}}
        catch (err) {
          window.aojError(err);
        }`)

      //@ts-ignore
      delete iframeWindow.aojError;
    } catch (error) {
      setIsValid(false);
    }
  };

  return (
    <Box className='App' style={{ minHeight: '100%', backgroundColor: NeutralColors.gray100, }}>
      <PanelGroup autoSaveId="example" direction="horizontal" style={{ position: 'fixed', height: '100%', top: '0px' }}>
        <Panel defaultSize={25}>
          <Stack style={{ height: '100%' }}>
            <Stack horizontal tokens={{ padding: '10px' }}>
              {selectionBtns.map(x =>
                <DefaultButton {...selectedEditor === x.value ? { primary: true } : undefined}
                  onClick={() => setSelectedEditor(x.value)}
                >
                  {x.label}
                </DefaultButton>)}
            </Stack>
            <Editor
              code={htmlCode}
              visible={selectedEditor === "html"}
              language='html'
              onChange={(props) => renderCode(props || "", cssCode, jsCode)}
            />
            <Editor
              code={cssCode}
              visible={selectedEditor === "css"}
              language='css'
              onChange={(props) => renderCode(htmlCode, props || "", jsCode)}
            />
            <Editor
              code={jsCode}
              visible={selectedEditor === "js"}
              language='javascript'
              onChange={(props) => renderCode(htmlCode, cssCode, props || "")}
            />
          </Stack>
        </Panel>
        <PanelResizeHandle />
        <Panel>
          <Stack style={{ height: '100%' }}>
            <Stack horizontal tokens={{ padding: '10px' }}>
              <DefaultButton disabled>
                Preview:
              </DefaultButton>
            </Stack>
            <Box padding={'15px'} height={"100%"}>
              {!isValid && <span style={{ fontSize: '20px', color: 'darkred' }}>Invalid HTML/JSX syntax</span>}
              <iframe ref={iframeRef}
                style={{
                  display: !isValid ? 'none' : 'block',
                  width: '100%',
                  height: '100%',
                  border: '0px'
                }}
              ></iframe>
            </Box>
          </Stack>
        </Panel>
      </PanelGroup>
    </Box>
  );
}

export default App;
