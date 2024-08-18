import { ReactNode } from "react";

function Flex(props: React.CSSProperties & React.DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement> & { children?: ReactNode | ReactNode[] }) {
    return <div style={{ display: 'flex', ...props }} {...props}>
        {props.children}
    </div>;
}

export default Flex;