# Design Support Library

### Ace Editor

```html
<ace-editor options="options" theme="theme" mode="mode"></ace-editor>
```

By using applyText() you can change the content of editor.
By using readText() you can get the content of editor.
By using resize() you can resize the editor.

### Dialog

```html
<design-dialog title="Title of dialog" (close)="onClose event">
Should put the content of dialog here.
Dialog has no content margin.
Dialog's header's color is #222.
</design-dialog>
```

### Notice Area & Notice History

Notice area is an area to show notices with a remove button. Notice history is an area to show notices if you did not click remove button in a notice after 5 second it showed in screen.

```html
<design-notice-area></design-notice-area>
<design-notice-history></design-notice-history>
```

When send new notice, you should send a new message to MessageService with:
```typescript
{ tag: TAG.Notice, value: Notice }
```
Strcture notice should be:
```typescript
{  icon: string, title: string, time: Date, content: string; }
```
TAG is a structure in /util.ts.

### Panel

Panel is an area with an icon at the top-left corner, title at top, and multiple actions at the bottom.

```html
<design-panel title="Title of this panel" icon="Material icon of this panel">
Should put the content of panel here.
<design-panel-action title="Title of this action"></design-panel-action>
</design-panel>
```

Action and its panel will have extra style after clicked them. When clicking, action will call synchronized event "TAG.ChangeAction" first with a CanceledEventArgs, if that variable returns true, action will send a message about change active action, if not, action will cancel this calling.

### Treeview

```html
<design-tree prefix="Prefix when send click message" data="Data of this tree" hideTitle="Should hide title of this tree node"></design-tree>
```

Treeview will show data as a tree, after each node was clicked, it will send a message "TAG.TreeClick" with prefix and value of the id of this node (in data). Data is a strcture of '/design-support/tree/tree.ts' named "Tree".