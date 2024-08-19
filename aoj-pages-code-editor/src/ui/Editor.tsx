import { Editor as MonacoEditor } from "@monaco-editor/react";
import { editor } from "monaco-editor";
import { useState } from "react";

interface EditorProps {
    visible: boolean,
    code: string,
    language: 'html' | 'css' | 'javascript',
    onChange: (v?: string) => any 
}

function Editor(props: EditorProps) {
    const [editorInstance, setEditorInstance] = useState<editor.IStandaloneCodeEditor>();
    

    return <MonacoEditor 
        onMount={(e) => setEditorInstance(e)}
        wrapperProps={{
            style: {
                display: props.visible ? 'flex' : 'none',
                position: 'relative',
                textAlign: 'initial',
                width: '100%',
                height: '100%',
            }
        }}
        language={props.language}
        value={props.code}
        theme='vs-dark'
        onChange={props.onChange}
        options={{
            autoClosingBrackets: 'always',
            autoClosingOvertype: 'always'
        }}
    />;
}

export default Editor;