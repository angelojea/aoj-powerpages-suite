import { ReactNode } from "react";

function Box(props: React.CSSProperties & React.DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement> & { children?: ReactNode | ReactNode[] }) {
    return <div style={props} {...props}>
        {props.children}
    </div>;
}

export default Box;