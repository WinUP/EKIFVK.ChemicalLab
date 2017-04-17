export interface Tree {
    title: string,
    children: Tree[],
    elements: { id: number, title: string }[]
}