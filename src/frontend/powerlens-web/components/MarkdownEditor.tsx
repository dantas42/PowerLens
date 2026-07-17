"use client";

import dynamic from "next/dynamic";

const MDXEditor = dynamic(() => import("@mdxeditor/editor").then((mod) => mod.MDXEditor), { ssr: false });

type MarkdownEditorProps = {
  value: string;
  onChange: (value: string) => void;
};

export function MarkdownEditor({ value, onChange }: MarkdownEditorProps) {
  return (
    <div className="overflow-hidden rounded-md border border-line bg-white">
      <MDXEditor
        markdown={value}
        onChange={onChange}
        plugins={[]}
        contentEditableClassName="min-h-[360px] px-4 py-3 text-sm leading-6 outline-none"
      />
    </div>
  );
}
